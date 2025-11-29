namespace TestOrderService.Application.DTOs
{
    public class TestResultDto
    {
        public Guid TestResultId { get; set; }

        public double Value { get; set; }

        public string Flag { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int TestDefinitionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public Guid? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? TestResultDescription { get; set; }

        public string? TestName { get; set; }

        public string? Unit { get; set; }

        public List<TestResultCommentDto> TestResultComments { get; set; } = new List<TestResultCommentDto>();
    }
}
