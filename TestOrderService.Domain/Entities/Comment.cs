using Microsoft.EntityFrameworkCore;
namespace TestOrderService.Domain.Entities
{
    [Index(nameof(CreateById), nameof(UpdateById), nameof(TestOrderId))]
    public class Comment
    {
        public Guid CommentId { get; set; }

        public string Content { get; set; } = "";

        public Guid CreateById { get; set; }

        public string CreateByName { get; set; } = "";

        public DateTime CreateAt { get; set; }

        public Guid? UpdateById { get; set; }

        public string UpdateByName { get; set; } = "";

        public DateTime? UpdateAt { get; set; }

        public Guid TestOrderId { get; set; }

        public TestOrder? TestOrder { get; set; }
    }
}
