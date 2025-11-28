using System.Text.Json.Serialization;
namespace TestOrderService.API.DTOs.AIReview
{

    public class FlaggedParameterDto
    {
        [JsonPropertyName("parameter")]
        public string Parameter { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public double Value { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // "Low" or "High"
        [JsonPropertyName("normal_range")]
        public string NormalRange { get; set; } = string.Empty;
    }

    public class AIReviewResponseDto
    {
        [JsonPropertyName("prediction")]
        public string Prediction { get; set; } = string.Empty; // "Normal", "Abnormal", "Critical"
        [JsonPropertyName("prediction_code")]
        public int PredictionCode { get; set; } // 0=Normal, 1=Abnormal, 2=Critical
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; } // 0-100
        [JsonPropertyName("probabilities")]
        public Dictionary<string, double> Probabilities { get; set; } = new Dictionary<string, double>();
        [JsonPropertyName("flagged_parameters")]
        public List<FlaggedParameterDto> FlaggedParameters { get; set; } = new List<FlaggedParameterDto>();
        [JsonPropertyName("suggestions")]
        public List<string> Suggestions { get; set; } = new List<string>();
    }
}
