using Microsoft.EntityFrameworkCore;
namespace TestOrderService.Domain.Entities
{
    [Index(nameof(CreateById), nameof(UpdateById), nameof(TestResultId))]
    public class TestResultComment
    {
        public Guid TestResultCommentId { get; set; }

        public string Content { get; set; } = "";

        public Guid CreateById { get; set; }

        public string CreateByName { get; set; } = "";

        public DateTime CreateAt { get; set; }

        public Guid? UpdateById { get; set; }

        public string UpdateByName { get; set; } = "";

        public DateTime? UpdateAt { get; set; }

        public Guid TestResultId { get; set; }

        public TestResult? TestResult { get; set; }
    }
}
