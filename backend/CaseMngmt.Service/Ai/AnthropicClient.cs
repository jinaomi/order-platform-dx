using CaseMngmt.Models.Ai;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CaseMngmt.Service.Ai
{
    public class AnthropicClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnthropicClient> _logger;
        private const int MaxRetries = 2;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public AnthropicClient(HttpClient httpClient, ILogger<AnthropicClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AnthropicResponse?> CreateMessageAsync(AnthropicRequest request, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(request, SerializerOptions);

            for (var attempt = 0; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using var response = await _httpClient.PostAsync("v1/messages", content, cancellationToken);
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonSerializer.Deserialize<AnthropicResponse>(body, SerializerOptions);
                    }

                    var isRetryable = response.StatusCode == (HttpStatusCode)429 || (int)response.StatusCode >= 500;
                    _logger.LogError("Anthropic API error {StatusCode}: {Body}", response.StatusCode, body);

                    if (!isRetryable || attempt == MaxRetries)
                    {
                        return null;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt + 1)), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Anthropic API call failed (attempt {Attempt})", attempt);
                    if (attempt == MaxRetries)
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}
