namespace TestOrderService.Domain.Entities
{
    public class TestResult
    {
        public Guid TestResultId { get; set; }

        public double Value { get; set; }

        public string Flag { get; set; } = string.Empty;

        public TestResultStatus Status { get; set; }

        public int TestDefinitionId { get; set; }

        public Guid TestOrderId { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public Guid? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? TestResultDescription { get; set; }

        public List<TestResultComment> TestResultComments { get; set; } = [];

        public TestOrder? TestOrder { get; set; }
    }

    public enum TestResultStatus
    {
        Completed, Cancelled, Reviewed,
        AIReviewed
    }
}
