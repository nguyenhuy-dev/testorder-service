using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.IntegrationEvents
{
    public class TestResultCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid TestResultId { get; set; }

        public double Value { get; set; }

        public int TestDefinitionId { get; set; }

        public Guid TestOrderId { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime CreateAt { get; set; }
    }
}
