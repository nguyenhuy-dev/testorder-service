
using System.ComponentModel.DataAnnotations;

namespace TestOrderService.Domain.Entities
{
    public class TestOrder
    {
        [Key]
        public Guid TestOrderId { get; set; }

        public Guid PatientId { get; set; }

        public StatusTestOrder Status { get; set; } = default!;

        public DateTime CreateAt { get; set; }

        public Guid CreateById { get; set; }

        public DateTime RunAt { get; set; }

        // Doctor
        public Guid RunById { get; set; }

        public DateTime? UpdateAt { get; set; }

        public Guid? UpdateById { get; set; }

        // Patient's signs.
        public string? TestOrderPatientDescription { get; set; }

        public Guid TestTypeId { get; set; }

    }

    public enum StatusTestOrder
    {
        Pending,
        Completed,
        Rejected
    }

}
