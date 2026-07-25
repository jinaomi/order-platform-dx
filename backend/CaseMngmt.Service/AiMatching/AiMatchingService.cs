using CaseMngmt.Models.Ai;
using CaseMngmt.Models.AiMatching;
using CaseMngmt.Models.Orders;
using CaseMngmt.Models.Products;
using CaseMngmt.Repository.AiMatching;
using CaseMngmt.Repository.Orders;
using CaseMngmt.Repository.Products;
using CaseMngmt.Service.Ai;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CaseMngmt.Service.AiMatching
{
    public class AiMatchingService : IAiMatchingService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRiskRepository _riskRepository;
        private readonly AnthropicClient _anthropicClient;
        private readonly ILogger<AiMatchingService> _logger;

        private const string ModelId = "claude-opus-4-8";
        private const string RiskSufficient = "Sufficient";
        private const string RiskWarning = "Warning";
        private const string RiskInsufficient = "Insufficient";
        private const int DefaultLeadDays = 7;

        public AiMatchingService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IOrderRiskRepository riskRepository,
            AnthropicClient anthropicClient,
            ILogger<AiMatchingService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _riskRepository = riskRepository;
            _anthropicClient = anthropicClient;
            _logger = logger;
        }

        public async Task<OrderRiskSummaryViewModel?> RunMatchingAsync(Guid orderId, Guid companyId, Guid currentUserId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, companyId);
            if (order == null)
            {
                return null;
            }

            var products = await _productRepository.GetAllAsync(companyId);
            var today = DateTime.UtcNow.Date;
            var leadDays = order.RequestedDeliveryDate.HasValue
                ? Math.Max((order.RequestedDeliveryDate.Value.Date - today).Days, 0)
                : DefaultLeadDays;

            var lineAssessments = new List<LineAssessment>();
            foreach (var item in order.OrderItems)
            {
                var product = item.ProductId.HasValue
                    ? products.FirstOrDefault(p => p.Id == item.ProductId.Value)
                    : products.FirstOrDefault(p => p.Name.Trim().Equals(item.ProductNameRaw.Trim(), StringComparison.OrdinalIgnoreCase));

                lineAssessments.Add(BuildDeterministicAssessment(item, product, leadDays));
            }

            await EnrichWithAiReasoningAsync(order, lineAssessments);

            await _riskRepository.SoftDeleteByOrderIdAsync(orderId);

            var evaluatedAt = DateTime.UtcNow;
            var entities = lineAssessments.Select(a => new OrderRiskLineResult
            {
                Name = $"{order.OrderNumber}-{a.Item.ProductNameRaw}",
                OrderId = order.Id,
                OrderItemId = a.Item.Id,
                RiskLevel = a.RiskLevel,
                Reasoning = a.Reasoning,
                SuggestedAction = a.SuggestedAction,
                EvaluatedAt = evaluatedAt,
                CreatedBy = currentUserId,
                UpdatedBy = currentUserId
            }).ToList();

            await _riskRepository.AddRangeAsync(entities);

            await UpdateOrderStatusAsync(order, lineAssessments, companyId, currentUserId);

            return BuildSummary(orderId, lineAssessments);
        }

        public async Task<OrderRiskSummaryViewModel?> GetLatestAsync(Guid orderId, Guid companyId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, companyId);
            if (order == null)
            {
                return null;
            }

            var results = await _riskRepository.GetByOrderIdAsync(orderId);
            if (results.Count == 0)
            {
                return null;
            }

            var itemNames = order.OrderItems.ToDictionary(i => i.Id, i => i.ProductNameRaw);
            var lines = results.Select(r => new OrderRiskLineResultViewModel
            {
                OrderItemId = r.OrderItemId,
                ProductNameRaw = itemNames.GetValueOrDefault(r.OrderItemId),
                RiskLevel = r.RiskLevel,
                Reasoning = r.Reasoning,
                SuggestedAction = r.SuggestedAction,
                EvaluatedAt = r.EvaluatedAt
            }).ToList();

            return new OrderRiskSummaryViewModel
            {
                OrderId = orderId,
                OverallRiskLevel = ComputeOverallRisk(results.Select(r => r.RiskLevel)),
                Lines = lines
            };
        }

        private static LineAssessment BuildDeterministicAssessment(OrderItem item, Product? product, int leadDays)
        {
            if (product == null)
            {
                return new LineAssessment(item, product, RiskWarning,
                    "商品マスタに登録がないため、在庫・生産能力を自動確認できませんでした。",
                    "手動で在庫状況をご確認ください。");
            }

            var availableWithCapacity = product.StockQuantity + (product.ProductionCapacityPerDay ?? 0) * leadDays;

            string riskLevel;
            if (item.Quantity <= product.StockQuantity)
            {
                riskLevel = RiskSufficient;
            }
            else if (item.Quantity <= availableWithCapacity)
            {
                riskLevel = RiskWarning;
            }
            else
            {
                riskLevel = RiskInsufficient;
            }

            return new LineAssessment(item, product, riskLevel, string.Empty, null);
        }

        private async Task EnrichWithAiReasoningAsync(Order order, List<LineAssessment> assessments)
        {
            var payloadItems = assessments.Select(a => new
            {
                order_item_id = a.Item.Id.ToString(),
                product_name = a.Item.ProductNameRaw,
                quantity = a.Item.Quantity,
                unit = a.Product?.UnitOfMeasure ?? "",
                risk_level = a.RiskLevel,
                stock_quantity = a.Product?.StockQuantity,
                production_capacity_per_day = a.Product?.ProductionCapacityPerDay,
            }).ToList();

            var userContent = JsonSerializer.Serialize(new
            {
                order_number = order.OrderNumber,
                requested_delivery_date = order.RequestedDeliveryDate?.ToString("yyyy-MM-dd"),
                items = payloadItems
            });

            var request = new AnthropicRequest
            {
                Model = ModelId,
                MaxTokens = 1500,
                System = "あなたは製造業向け受注管理システムのAIアシスタントです。各受注明細行について、在庫数量・生産能力から算出済みのリスク判定(risk_level)を踏まえ、現場担当者にとって分かりやすい日本語で状況説明と推奨対応を簡潔に生成してください。risk_level自体は既に確定しているため変更せず、説明文の生成のみ行ってください。",
                Messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage { Role = "user", Content = userContent }
                },
                Tools = new List<AnthropicTool>
                {
                    new AnthropicTool
                    {
                        Name = "provide_risk_explanations",
                        Description = "各受注明細行に対するリスク説明と推奨対応を日本語で提供する",
                        InputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                items = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            order_item_id = new { type = "string" },
                                            reasoning = new { type = "string", description = "リスク状況の説明（日本語、1〜2文）" },
                                            suggested_action = new { type = "string", description = "推奨される対応（日本語、1文。特になければ空文字）" }
                                        },
                                        required = new[] { "order_item_id", "reasoning", "suggested_action" }
                                    }
                                }
                            },
                            required = new[] { "items" }
                        }
                    }
                },
                ToolChoice = new AnthropicToolChoice { Type = "tool", Name = "provide_risk_explanations" }
            };

            AnthropicResponse? response = null;
            try
            {
                response = await _anthropicClient.CreateMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI照合: Anthropic API call threw an exception");
            }

            var toolUseBlock = response?.Content.FirstOrDefault(c => c.Type == "tool_use");
            if (toolUseBlock != null && toolUseBlock.Input.TryGetProperty("items", out var itemsElement))
            {
                foreach (var itemElement in itemsElement.EnumerateArray())
                {
                    if (!itemElement.TryGetProperty("order_item_id", out var idProp) ||
                        !Guid.TryParse(idProp.GetString(), out var orderItemId))
                    {
                        continue;
                    }

                    var assessment = assessments.FirstOrDefault(a => a.Item.Id == orderItemId);
                    if (assessment == null)
                    {
                        continue;
                    }

                    var reasoning = itemElement.TryGetProperty("reasoning", out var reasoningProp)
                        ? reasoningProp.GetString() ?? string.Empty
                        : string.Empty;
                    var suggestedAction = itemElement.TryGetProperty("suggested_action", out var actionProp)
                        ? actionProp.GetString()
                        : null;

                    assessment.Reasoning = string.IsNullOrWhiteSpace(reasoning) ? GetFallbackReasoning(assessment.RiskLevel) : reasoning;
                    assessment.SuggestedAction = string.IsNullOrWhiteSpace(suggestedAction) ? null : suggestedAction;
                }
            }

            // Fill in fallback reasoning for any line the AI response didn't cover (or if the AI call failed entirely)
            foreach (var assessment in assessments.Where(a => string.IsNullOrEmpty(a.Reasoning)))
            {
                assessment.Reasoning = GetFallbackReasoning(assessment.RiskLevel);
            }
        }

        private static string GetFallbackReasoning(string riskLevel)
        {
            return riskLevel switch
            {
                RiskSufficient => "現在の在庫のみで対応可能です。",
                RiskWarning => "在庫のみでは不足していますが、生産能力を活用すれば納期までに対応できる見込みです。",
                RiskInsufficient => "在庫と生産能力を合わせても不足する可能性があります。至急ご確認ください。",
                _ => "状況を確認してください。"
            };
        }

        private async Task UpdateOrderStatusAsync(Order order, List<LineAssessment> assessments, Guid companyId, Guid currentUserId)
        {
            var hasInsufficientRisk = assessments.Any(a => a.RiskLevel == RiskInsufficient);

            if (hasInsufficientRisk && order.Status == "Confirmed")
            {
                await _orderRepository.UpdateStatusAsync(order.Id, companyId, "RiskFlagged", currentUserId);
            }
            else if (!hasInsufficientRisk && order.Status == "RiskFlagged")
            {
                await _orderRepository.UpdateStatusAsync(order.Id, companyId, "Confirmed", currentUserId);
            }
        }

        private static OrderRiskSummaryViewModel BuildSummary(Guid orderId, List<LineAssessment> assessments)
        {
            return new OrderRiskSummaryViewModel
            {
                OrderId = orderId,
                OverallRiskLevel = ComputeOverallRisk(assessments.Select(a => a.RiskLevel)),
                Lines = assessments.Select(a => new OrderRiskLineResultViewModel
                {
                    OrderItemId = a.Item.Id,
                    ProductNameRaw = a.Item.ProductNameRaw,
                    RiskLevel = a.RiskLevel,
                    Reasoning = a.Reasoning,
                    SuggestedAction = a.SuggestedAction,
                    EvaluatedAt = DateTime.UtcNow
                }).ToList()
            };
        }

        private static string ComputeOverallRisk(IEnumerable<string> riskLevels)
        {
            var levels = riskLevels.ToList();
            if (levels.Contains(RiskInsufficient)) return RiskInsufficient;
            if (levels.Contains(RiskWarning)) return RiskWarning;
            return RiskSufficient;
        }

        private class LineAssessment
        {
            public LineAssessment(OrderItem item, Product? product, string riskLevel, string reasoning, string? suggestedAction)
            {
                Item = item;
                Product = product;
                RiskLevel = riskLevel;
                Reasoning = reasoning;
                SuggestedAction = suggestedAction;
            }

            public OrderItem Item { get; }
            public Product? Product { get; }
            public string RiskLevel { get; set; }
            public string Reasoning { get; set; }
            public string? SuggestedAction { get; set; }
        }
    }
}
