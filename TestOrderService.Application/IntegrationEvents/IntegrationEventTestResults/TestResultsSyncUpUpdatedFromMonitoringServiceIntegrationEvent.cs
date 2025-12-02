using TestOrderService.Application.Interfaces.Events;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.IntegrationEvents.TestResultIntegrationEvents
{
    public class TestResultsSyncUpUpdatedFromMonitoringServiceIntegrationEvent : IntegrationEvent
    {
        public Guid TestOrderId { get; set; }

        public List<TestResult> TestResults { get; set; } = [];

        public Guid UpdatedBy { get; set; }
    }
}
