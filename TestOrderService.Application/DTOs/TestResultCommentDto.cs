namespace TestOrderService.Application.DTOs
{
    public class TestResultCommentDto
    {
        public Guid TestResultCommentId { get; set; }

        public string Content { get; set; } = default!;

        public Guid CreateById { get; set; }

        public string CreateByName { get; set; } = default!;

        public DateTime CreateAt { get; set; }

        public Guid? UpdateById { get; set; }

        public string UpdateByName { get; set; } = default!;

        public DateTime? UpdateAt { get; set; }

        public Guid TestResultId { get; set; }
    }
}
