using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents
{
    public class TestOrderDeletedIntegrationEvent : IntegrationEvent
    {
        public Guid TestOrderId { get; set; }
    }
}
