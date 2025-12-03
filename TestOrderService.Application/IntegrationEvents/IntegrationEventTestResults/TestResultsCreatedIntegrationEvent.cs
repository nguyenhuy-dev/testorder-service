using TestOrderService.Application.Interfaces.Events;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.IntegrationEvents.IntegrationEventTestResults
{
    public class TestResultsCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid TestOrderId { get; set; }

        public List<TestResult> TestResults { get; set; } = [];

        public Guid CreatedBy { get; set; }
    }
}
