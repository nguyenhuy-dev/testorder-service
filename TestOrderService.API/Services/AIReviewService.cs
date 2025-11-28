using System.Text;
using System.Text.Json;
using TestOrderService.API.DTOs.AIReview;
namespace TestOrderService.API.Services
{
    public interface IAIReviewService
    {
        Task<AIReviewResponseDto?> ReviewCBCAsync(AIReviewRequestDto request, CancellationToken cancellationToken = default);
    }

    public class AIReviewService : IAIReviewService
    {
        private readonly string _aiServiceUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIReviewService> _logger;

        public AIReviewService(HttpClient httpClient, IConfiguration configuration, ILogger<AIReviewService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiServiceUrl = configuration["AIReviewService:BaseUrl"] ?? "http://localhost:8088";

            _httpClient.BaseAddress = new Uri(_aiServiceUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<AIReviewResponseDto?> ReviewCBCAsync(AIReviewRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Calling AI Review Service for CBC analysis");

                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/review", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("AI Review Service returned error: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<AIReviewResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("AI Review completed: {Prediction} ({Confidence}% confidence)",
                    result?.Prediction, result?.Confidence);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to AI Review Service at {Url}", _aiServiceUrl);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI Review");
                return null;
            }
        }
    }
}
