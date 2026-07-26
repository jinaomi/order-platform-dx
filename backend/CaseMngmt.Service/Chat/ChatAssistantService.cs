using CaseMngmt.Models.Ai;
using CaseMngmt.Models.Chat;
using CaseMngmt.Repository.Customers;
using CaseMngmt.Repository.Invoices;
using CaseMngmt.Repository.Orders;
using CaseMngmt.Repository.Products;
using CaseMngmt.Service.Ai;
using CaseMngmt.Service.Dashboard;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CaseMngmt.Service.Chat
{
    public class ChatAssistantService : IChatAssistantService
    {
        private readonly AnthropicClient _anthropicClient;
        private readonly IDashboardService _dashboardService;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ILogger<ChatAssistantService> _logger;

        private const string ModelId = "claude-opus-4-8";
        private const int MaxHistoryTurns = 10;
        private const int MaxToolIterations = 5;
        private const string FallbackReply = "申し訳ございません、現在AIアシスタントに接続できません。しばらくしてからもう一度お試しください。";

        private static readonly JsonSerializerOptions ToolResultSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly List<AnthropicTool> Tools = new()
        {
            new AnthropicTool
            {
                Name = "get_dashboard_summary",
                Description = "受注件数・受注金額合計・請求済み金額・リスクあり受注件数・ステータス別内訳・月別売上・取引先別/商品別売上TOP5など、経営ダッシュボードの集計データを取得する。",
                InputSchema = new { type = "object", properties = new { } }
            },
            new AnthropicTool
            {
                Name = "search_orders",
                Description = "受注を検索する。ステータスや取引先名で絞り込み可能。各受注の明細（商品名・数量・金額）も含まれる。",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        status = new { type = "string", description = "絞り込むステータス（Draft, PendingReview, Confirmed, RiskFlagged, Invoiced, Cancelled のいずれか）" },
                        customerName = new { type = "string", description = "絞り込む取引先名（部分一致）" }
                    }
                }
            },
            new AnthropicTool
            {
                Name = "search_products",
                Description = "商品・在庫を検索する。在庫が少ない商品のみに絞り込むことも可能。",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        lowStockOnly = new { type = "boolean", description = "true の場合、在庫数量が1日あたり生産能力以下の商品のみ返す" },
                        nameContains = new { type = "string", description = "絞り込む商品名（部分一致）" }
                    }
                }
            },
            new AnthropicTool
            {
                Name = "search_invoices",
                Description = "請求書を検索する。ステータス（Draft, Issued, Paid, Overdue）で絞り込み可能。",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        status = new { type = "string", description = "絞り込むステータス（Draft, Issued, Paid, Overdue のいずれか）" }
                    }
                }
            }
        };

        public ChatAssistantService(
            AnthropicClient anthropicClient,
            IDashboardService dashboardService,
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IInvoiceRepository invoiceRepository,
            ILogger<ChatAssistantService> logger)
        {
            _anthropicClient = anthropicClient;
            _dashboardService = dashboardService;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        public async Task<string> AskAsync(Guid companyId, string message, List<ChatHistoryTurn> history)
        {
            var messages = new List<AnthropicMessage>();
            foreach (var turn in history.TakeLast(MaxHistoryTurns))
            {
                messages.Add(new AnthropicMessage { Role = turn.Role, Content = turn.Content });
            }
            messages.Add(new AnthropicMessage { Role = "user", Content = message });

            const string systemPrompt =
                "あなたは製造業向けSME(中小企業)向け受注管理システムの社内アシスタントです。" +
                "受注・在庫・請求・売上に関する質問に、提供されたツールで取得した実データのみに基づいて日本語で簡潔に答えてください。" +
                "データにない数字を推測・捏造しないでください。ツールで取得できない範囲の質問（このシステムのデータに関係ない質問）には、丁寧にその旨を伝えて断ってください。" +
                "回答はプレーンテキストのチャット画面に表示されるため、Markdown表（|---|のような罫線）や見出し記号（##）は使わず、通常の文章と「・」による簡単な箇条書きのみで構成してください。";

            for (var iteration = 0; iteration < MaxToolIterations; iteration++)
            {
                var request = new AnthropicRequest
                {
                    Model = ModelId,
                    MaxTokens = 1500,
                    System = systemPrompt,
                    Messages = messages,
                    Tools = Tools
                };

                AnthropicResponse? response;
                try
                {
                    response = await _anthropicClient.CreateMessageAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Chat AI: Anthropic API call threw an exception");
                    return FallbackReply;
                }

                if (response == null)
                {
                    _logger.LogError("Chat AI: Anthropic API call returned null");
                    return FallbackReply;
                }

                if (response.StopReason != "tool_use")
                {
                    var text = string.Join("\n", response.Content.Where(c => c.Type == "text").Select(c => c.Text));
                    return string.IsNullOrWhiteSpace(text) ? FallbackReply : text;
                }

                messages.Add(new AnthropicMessage { Role = "assistant", Content = response.Content.Select(ToRequestContentBlock).ToList() });

                var toolResults = new List<object>();
                foreach (var block in response.Content.Where(c => c.Type == "tool_use"))
                {
                    var resultJson = await ExecuteToolAsync(block.Name ?? string.Empty, block.Input, companyId);
                    toolResults.Add(new { type = "tool_result", tool_use_id = block.Id, content = resultJson });
                }
                messages.Add(new AnthropicMessage { Role = "user", Content = toolResults });
            }

            _logger.LogError("Chat AI: exceeded max tool iterations ({MaxToolIterations})", MaxToolIterations);
            return FallbackReply;
        }

        private async Task<string> ExecuteToolAsync(string toolName, JsonElement input, Guid companyId)
        {
            try
            {
                switch (toolName)
                {
                    case "get_dashboard_summary":
                        return await GetDashboardSummaryAsync(companyId);
                    case "search_orders":
                        return await SearchOrdersAsync(input, companyId);
                    case "search_products":
                        return await SearchProductsAsync(input, companyId);
                    case "search_invoices":
                        return await SearchInvoicesAsync(input, companyId);
                    default:
                        return JsonSerializer.Serialize(new { error = $"unknown tool: {toolName}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chat AI: tool execution failed for {ToolName}", toolName);
                return JsonSerializer.Serialize(new { error = "ツールの実行に失敗しました" });
            }
        }

        private async Task<string> GetDashboardSummaryAsync(Guid companyId)
        {
            var summary = await _dashboardService.GetSummaryAsync(companyId);
            return JsonSerializer.Serialize(summary, ToolResultSerializerOptions);
        }

        private async Task<string> SearchOrdersAsync(JsonElement input, Guid companyId)
        {
            var status = GetStringProperty(input, "status");
            var customerName = GetStringProperty(input, "customerName");

            Guid? customerId = null;
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                var customers = await _customerRepository.GetAllAsync(companyId);
                var match = customers.FirstOrDefault(c => c.Name.Contains(customerName, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    return JsonSerializer.Serialize(new { orders = Array.Empty<object>(), note = $"取引先名「{customerName}」に一致する取引先が見つかりませんでした" });
                }
                customerId = match.Id;
            }

            var result = await _orderRepository.GetAllAsync(companyId, status, customerId, null, null, 50, 1);
            var orders = (result?.Items ?? Enumerable.Empty<Models.Orders.Order>()).Select(o => new
            {
                orderNumber = o.OrderNumber,
                customerName = o.Customer?.Name,
                orderDate = o.OrderDate,
                requestedDeliveryDate = o.RequestedDeliveryDate,
                status = o.Status,
                totalAmount = o.TotalAmount,
                items = o.OrderItems.Select(i => new { productName = i.ProductNameRaw, quantity = i.Quantity, unitPrice = i.UnitPrice, lineAmount = i.LineAmount })
            });

            return JsonSerializer.Serialize(new { orders }, ToolResultSerializerOptions);
        }

        private async Task<string> SearchProductsAsync(JsonElement input, Guid companyId)
        {
            var lowStockOnly = GetBoolProperty(input, "lowStockOnly");
            var nameContains = GetStringProperty(input, "nameContains");

            var products = await _productRepository.GetAllAsync(companyId);

            var query = products.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(nameContains))
            {
                query = query.Where(p => p.Name.Contains(nameContains, StringComparison.OrdinalIgnoreCase));
            }
            if (lowStockOnly == true)
            {
                query = query.Where(p => p.StockQuantity <= (p.ProductionCapacityPerDay ?? 0));
            }

            var productList = query.Select(p => new
            {
                name = p.Name,
                productCode = p.ProductCode,
                stockQuantity = p.StockQuantity,
                unitOfMeasure = p.UnitOfMeasure,
                productionCapacityPerDay = p.ProductionCapacityPerDay,
                unitPrice = p.UnitPrice
            });

            return JsonSerializer.Serialize(new { products = productList }, ToolResultSerializerOptions);
        }

        private async Task<string> SearchInvoicesAsync(JsonElement input, Guid companyId)
        {
            var status = GetStringProperty(input, "status");

            var result = await _invoiceRepository.GetAllAsync(companyId, null, status, null, null, null, 50, 1);
            var query = (result?.Items ?? Enumerable.Empty<Models.Invoices.Invoice>()).AsEnumerable();

            var invoices = query.Select(i => new
            {
                invoiceNumber = i.InvoiceNumber,
                customerName = i.Customer?.Name,
                orderNumber = i.Order?.OrderNumber,
                issueDate = i.IssueDate,
                dueDate = i.DueDate,
                totalAmount = i.TotalAmount,
                status = i.Status
            });

            return JsonSerializer.Serialize(new { invoices }, ToolResultSerializerOptions);
        }

        private static object ToRequestContentBlock(AnthropicContentBlock block)
        {
            if (block.Type == "tool_use")
            {
                return new { type = "tool_use", id = block.Id, name = block.Name, input = block.Input };
            }
            return new { type = "text", text = block.Text };
        }

        private static string? GetStringProperty(JsonElement input, string name)
        {
            if (input.ValueKind == JsonValueKind.Object && input.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String)
            {
                return el.GetString();
            }
            return null;
        }

        private static bool? GetBoolProperty(JsonElement input, string name)
        {
            if (input.ValueKind == JsonValueKind.Object && input.TryGetProperty(name, out var el) &&
                (el.ValueKind == JsonValueKind.True || el.ValueKind == JsonValueKind.False))
            {
                return el.GetBoolean();
            }
            return null;
        }
    }
}
