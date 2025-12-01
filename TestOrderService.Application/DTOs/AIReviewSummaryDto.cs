using System.Text.Json.Serialization;
namespace TestOrderService.Application.DTOs
{
    public class AIReviewSummaryDto
    {
        [JsonPropertyName("prediction")]
        public string? Prediction { get; set; }

        [JsonPropertyName("prediction_code")]
        public int PredictionCode { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("probabilities")]
        public ProbabilitiesDto? Probabilities { get; set; }

        [JsonPropertyName("flaggedParameters")]
        public List<FlaggedParameterDto>? FlaggedParameters { get; set; }

        [JsonPropertyName("suggestions")]
        public List<string>? Suggestions { get; set; }

        public string? RawText { get; set; }
    }


    public class ProbabilitiesDto
    {
        [JsonPropertyName("normal")]
        public double Normal { get; set; }

        [JsonPropertyName("abnormal")]
        public double Abnormal { get; set; }

        [JsonPropertyName("critical")]
        public double Critical { get; set; }
    }

    public class FlaggedParameterDto
    {
        [JsonPropertyName("parameter")]
        public string? Parameter { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("normalRange")]
        public string? NormalRange { get; set; }
    }
}
