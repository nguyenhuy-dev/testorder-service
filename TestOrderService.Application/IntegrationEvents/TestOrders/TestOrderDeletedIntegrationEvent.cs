using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents.TestOrders
{
    public class TestOrderDeletedIntegrationEvent : IntegrationEvent
    {
        public Guid TestOrderId { get; set; }
    }
}
