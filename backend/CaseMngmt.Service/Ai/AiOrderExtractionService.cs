using CaseMngmt.Models.Ai;
using CaseMngmt.Models.Orders;
using CaseMngmt.Repository.Customers;
using CaseMngmt.Repository.Products;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CaseMngmt.Service.Ai
{
    public class AiOrderExtractionService : IAiOrderExtractionService
    {
        private readonly AnthropicClient _anthropicClient;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<AiOrderExtractionService> _logger;

        private const string ModelId = "claude-opus-4-8";

        public AiOrderExtractionService(
            AnthropicClient anthropicClient,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            ILogger<AiOrderExtractionService> logger)
        {
            _anthropicClient = anthropicClient;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<OrderExtractionResult?> ExtractAsync(byte[] fileBytes, string mediaType, Guid companyId)
        {
            var base64Data = Convert.ToBase64String(fileBytes);
            var isPdf = mediaType == "application/pdf";

            object documentBlock = isPdf
                ? new
                {
                    type = "document",
                    source = new { type = "base64", media_type = mediaType, data = base64Data }
                }
                : new
                {
                    type = "image",
                    source = new { type = "base64", media_type = mediaType, data = base64Data }
                };

            var request = new AnthropicRequest
            {
                Model = ModelId,
                MaxTokens = 2000,
                System = "あなたは製造業向け受注管理システムのAIアシスタントです。アップロードされた注文書・受注書の画像またはPDFから、取引先名・受注日・希望納期・品目明細（品名・数量・単価）を読み取り、指定されたツールを使って構造化データとして返してください。数値は半角数字に変換してください。" +
                    "重要：該当箇所に本当に何も記載がない場合のみ空文字または0にしてください。文字が書かれているが手書きで不鮮明・崩し字・略称（例：「株式会社」を「（株）」と略しているなど）で確信が持てない場合は、絶対に空文字にせず、実際に書かれている通りの文字列をそのまま転記した上で、confidenceを低く設定してください。取引先名（customer_name）も品目と同様に、読み取れる限り必ず転記し、customer_name_confidenceで確信度を表現してください。",
                Messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage
                    {
                        Role = "user",
                        Content = new List<object>
                        {
                            documentBlock,
                            new { type = "text", text = "この受注書/注文書から情報を抽出してください。" }
                        }
                    }
                },
                Tools = new List<AnthropicTool>
                {
                    new AnthropicTool
                    {
                        Name = "extract_order",
                        Description = "受注書/注文書の画像またはPDFから受注情報を抽出する",
                        InputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                customer_name = new { type = "string", description = "取引先名。手書きが不鮮明・崩し字・略称（「（株）」等）でも、実際に書かれている通りの文字列をそのまま転記すること（推測で正式名称に変換したり、確信が持てないからと省略しないこと）。本当に何も記載がない場合のみ空文字" },
                                customer_name_confidence = new { type = "number", description = "取引先名の読み取りに対する信頼度（0.0〜1.0）。手書きが不鮮明・略称・崩し字などで確信が持てない場合は低い値を設定" },
                                order_date = new { type = "string", description = "受注日（YYYY-MM-DD形式）。読み取れない場合は空文字" },
                                requested_delivery_date = new { type = "string", description = "希望納期（YYYY-MM-DD形式）。読み取れない場合は空文字" },
                                items = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            product_name = new { type = "string", description = "品名" },
                                            quantity = new { type = "number", description = "数量" },
                                            unit_price = new { type = "number", description = "単価。読み取れない場合は0" },
                                            confidence = new { type = "number", description = "この行の抽出結果に対する信頼度（0.0〜1.0）" }
                                        },
                                        required = new[] { "product_name", "quantity", "confidence" }
                                    }
                                }
                            },
                            required = new[] { "customer_name", "customer_name_confidence", "items" }
                        }
                    }
                },
                ToolChoice = new AnthropicToolChoice { Type = "tool", Name = "extract_order" }
            };

            AnthropicResponse? response;
            try
            {
                response = await _anthropicClient.CreateMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "データ化: Anthropic API call threw an exception");
                return null;
            }

            var toolUseBlock = response?.Content.FirstOrDefault(c => c.Type == "tool_use");
            if (toolUseBlock == null)
            {
                _logger.LogError("データ化: Anthropic response did not contain a tool_use block");
                return null;
            }

            var result = new OrderExtractionResult();
            var input = toolUseBlock.Input;

            if (input.TryGetProperty("customer_name", out var customerNameEl))
            {
                var name = customerNameEl.GetString();
                result.CustomerNameGuess = string.IsNullOrWhiteSpace(name) ? null : name;
            }

            result.CustomerNameConfidence = input.TryGetProperty("customer_name_confidence", out var customerConfEl)
                ? customerConfEl.GetDouble()
                : 0.5;

            if (input.TryGetProperty("order_date", out var orderDateEl) &&
                DateTime.TryParse(orderDateEl.GetString(), out var orderDate))
            {
                result.OrderDateGuess = orderDate;
            }

            if (input.TryGetProperty("requested_delivery_date", out var deliveryDateEl) &&
                DateTime.TryParse(deliveryDateEl.GetString(), out var deliveryDate))
            {
                result.RequestedDeliveryDateGuess = deliveryDate;
            }

            var companyCustomers = await _customerRepository.GetAllAsync(companyId);
            if (!string.IsNullOrEmpty(result.CustomerNameGuess))
            {
                var normalizedGuess = NormalizeCompanyName(result.CustomerNameGuess);
                var matchedCustomer = companyCustomers.FirstOrDefault(c =>
                    NormalizeCompanyName(c.Name).Equals(normalizedGuess, StringComparison.OrdinalIgnoreCase));
                result.CustomerIdMatch = matchedCustomer?.Id;
            }

            var companyProducts = await _productRepository.GetAllAsync(companyId);
            if (input.TryGetProperty("items", out var itemsEl))
            {
                foreach (var itemEl in itemsEl.EnumerateArray())
                {
                    var productName = itemEl.TryGetProperty("product_name", out var nameEl) ? nameEl.GetString() ?? string.Empty : string.Empty;
                    var quantity = itemEl.TryGetProperty("quantity", out var qtyEl) ? qtyEl.GetDecimal() : 0;
                    var unitPrice = itemEl.TryGetProperty("unit_price", out var priceEl) ? priceEl.GetDecimal() : 0;
                    var confidence = itemEl.TryGetProperty("confidence", out var confEl) ? confEl.GetDouble() : 0.5;

                    var matchedProduct = companyProducts.FirstOrDefault(p =>
                        p.Name.Trim().Equals(productName.Trim(), StringComparison.OrdinalIgnoreCase));

                    result.Items.Add(new OrderExtractionItem
                    {
                        ProductNameRaw = productName,
                        ProductIdMatch = matchedProduct?.Id,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Confidence = confidence
                    });
                }
            }

            return result;
        }

        private static readonly string[] CompanySuffixVariants =
        {
            "株式会社", "（株）", "(株)", "㈱",
            "有限会社", "（有）", "(有)", "㈲"
        };

        private static string NormalizeCompanyName(string name)
        {
            var normalized = name.Trim();
            foreach (var suffix in CompanySuffixVariants)
            {
                normalized = normalized.Replace(suffix, string.Empty);
            }
            return normalized.Trim();
        }
    }
}
