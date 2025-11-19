using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace TestOrderService.Domain.Entities
{
    [Index(nameof(PatientId), nameof(ReviewId), nameof(CreateById), nameof(UpdateById))]
    public class TestOrder
    {
        [Key]
        public Guid TestOrderId { get; set; }

        public Guid PatientId { get; set; }

        public StatusTestOrder Status { get; set; } = default!;

        public Guid? ReviewId { get; set; }

        public DateTime? ReviewAt { get; set; }

        public Guid CreateById { get; set; }

        public DateTime CreateAt { get; set; }

        // Doctor
        public Guid RunById { get; set; }

        public DateTime? RunAt { get; set; }

        public Guid? UpdateById { get; set; }

        public DateTime? UpdateAt { get; set; }

        // Patient's signs.
        public string? TestOrderDescription { get; set; }
    }

    public enum StatusTestOrder { Pending, Completed, Rejected }
}
