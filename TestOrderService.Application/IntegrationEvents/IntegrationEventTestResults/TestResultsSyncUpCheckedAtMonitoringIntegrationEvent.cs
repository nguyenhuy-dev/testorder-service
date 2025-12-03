using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents.IntegrationEventTestResults
{
    public class TestResultsSyncUpCheckedAtMonitoringIntegrationEvent : IntegrationEvent
    {
        public List<Guid> TestOrderIds { get; set; } = [];

        public Guid UpdatedBy { get; set; }
    }
}
