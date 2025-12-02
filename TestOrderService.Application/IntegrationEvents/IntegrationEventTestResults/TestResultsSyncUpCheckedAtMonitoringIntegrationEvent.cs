using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents.TestResultIntegrationEvents
{
    public class TestResultsSyncUpCheckedAtMonitoringIntegrationEvent : IntegrationEvent
    {
        public List<Guid> TestOrderIds { get; set; } = [];

        public Guid UpdatedBy { get; set; }
    }
}
