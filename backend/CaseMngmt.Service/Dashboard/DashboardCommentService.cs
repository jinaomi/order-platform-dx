using CaseMngmt.Models.Ai;
using CaseMngmt.Models.Dashboard;
using CaseMngmt.Service.Ai;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CaseMngmt.Service.Dashboard
{
    public class DashboardCommentService : IDashboardCommentService
    {
        private readonly IDashboardService _dashboardService;
        private readonly AnthropicClient _anthropicClient;
        private readonly ILogger<DashboardCommentService> _logger;

        private const string ModelId = "claude-opus-4-8";

        public DashboardCommentService(
            IDashboardService dashboardService,
            AnthropicClient anthropicClient,
            ILogger<DashboardCommentService> logger)
        {
            _dashboardService = dashboardService;
            _anthropicClient = anthropicClient;
            _logger = logger;
        }

        public async Task<DashboardAiCommentViewModel?> GenerateCommentAsync(Guid companyId)
        {
            var summary = await _dashboardService.GetSummaryAsync(companyId);

            if (summary.TotalOrders == 0)
            {
                return null;
            }

            var userContent = JsonSerializer.Serialize(new
            {
                total_orders = summary.TotalOrders,
                total_order_amount = summary.TotalOrderAmount,
                total_invoiced_amount = summary.TotalInvoicedAmount,
                risk_flagged_count = summary.RiskFlaggedCount,
                order_funnel = summary.OrderFunnel.Select(f => new { status = f.Status, count = f.Count }),
                monthly_sales = summary.MonthlySales.Select(m => new { month = m.Month, order_count = m.OrderCount, total_amount = m.TotalAmount }),
                top_customers = summary.TopCustomers.Select(c => new { customer_name = c.CustomerName, order_count = c.OrderCount, total_amount = c.TotalAmount }),
                top_products = summary.TopProducts.Select(p => new { product_name = p.ProductName, total_quantity = p.TotalQuantity, total_amount = p.TotalAmount })
            });

            var request = new AnthropicRequest
            {
                Model = ModelId,
                MaxTokens = 1000,
                System = "あなたは製造業向けSME(中小企業)の経営者に助言する優秀な経営アドバイザーです。受注管理システムのダッシュボード集計データを渡すので、経営判断に役立つ簡潔な日本語コメントを生成してください。専門用語を避け、具体的な数字を引用しながら、経営者が次に何をすべきか分かるようにしてください。",
                Messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage { Role = "user", Content = userContent }
                },
                Tools = new List<AnthropicTool>
                {
                    new AnthropicTool
                    {
                        Name = "provide_dashboard_insight",
                        Description = "ダッシュボードの集計データに基づく経営コメントを提供する",
                        InputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                headline = new { type = "string", description = "全体状況を一言で表す見出し（日本語、1文）" },
                                highlights = new
                                {
                                    type = "array",
                                    description = "注目すべきポイント（2〜4個、それぞれ日本語で1文、具体的な数字を含める）",
                                    items = new { type = "string" }
                                },
                                recommendation = new { type = "string", description = "経営者への推奨アクション（日本語、1文）" }
                            },
                            required = new[] { "headline", "highlights", "recommendation" }
                        }
                    }
                },
                ToolChoice = new AnthropicToolChoice { Type = "tool", Name = "provide_dashboard_insight" }
            };

            AnthropicResponse? response;
            try
            {
                response = await _anthropicClient.CreateMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "経営判断コメント: Anthropic API call threw an exception");
                return null;
            }

            var toolUseBlock = response?.Content.FirstOrDefault(c => c.Type == "tool_use");
            if (toolUseBlock == null)
            {
                _logger.LogError("経営判断コメント: Anthropic response did not contain a tool_use block");
                return null;
            }

            var input = toolUseBlock.Input;
            var result = new DashboardAiCommentViewModel
            {
                Headline = input.TryGetProperty("headline", out var headlineEl) ? headlineEl.GetString() ?? string.Empty : string.Empty,
                Recommendation = input.TryGetProperty("recommendation", out var recEl) ? recEl.GetString() ?? string.Empty : string.Empty
            };

            if (input.TryGetProperty("highlights", out var highlightsEl))
            {
                result.Highlights = highlightsEl.EnumerateArray()
                    .Select(h => h.GetString() ?? string.Empty)
                    .Where(h => !string.IsNullOrWhiteSpace(h))
                    .ToList();
            }

            return result;
        }
    }
}
