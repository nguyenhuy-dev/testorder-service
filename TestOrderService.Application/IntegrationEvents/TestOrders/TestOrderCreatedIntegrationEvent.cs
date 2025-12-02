using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents.TestOrders
{
    public class TestOrderCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid TestOrderId { get; set; }

        public Guid PatientId { get; set; }
    }
}
