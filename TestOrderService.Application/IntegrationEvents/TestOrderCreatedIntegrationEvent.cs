using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents
{
    public class TestOrderCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid TestOrderId { get; set; }

        public Guid PatientId { get; set; }
    }
}
