using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents
{
    public class TestOrderUpdatedIntegrationEvent : IntegrationEvent
    {
        public Guid TestOrderId { get; set; }

        public Guid PatientId { get; set; }

        public string? Status { get; set; }

        public Guid? RunById { get; set; }

        public DateTime? RunAt { get; set; }
    }
}

