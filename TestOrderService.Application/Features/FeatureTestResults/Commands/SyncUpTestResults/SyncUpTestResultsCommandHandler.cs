using Microsoft.Extensions.Logging;
using TestOrderService.Application.IntegrationEvents.TestResultIntegrationEvents;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.FeatureTestResults.Commands.SyncUpTestResults
{
    public class SyncUpTestResultsCommandHandler(
        ITestOrderRepository testOrderRepository,
        IEventPublisher eventPublisher,
        ILogger<SyncUpTestResultsCommandHandler> logger)
        : ICommandHandler<SyncUpTestResultsCommand, List<TestOrder>>
    {

        private readonly IEventPublisher _eventPublisher = eventPublisher;

        private readonly ILogger<SyncUpTestResultsCommandHandler> _logger = logger;
        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;

        public async Task<List<TestOrder>> Handle(SyncUpTestResultsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending request sync up test results to Monitoring Service...");

            var testOrdersFiltered = await _testOrderRepository.GetTestOrdersCompletedButMissingTestResults(cancellationToken);
            if (!testOrdersFiltered.Any())
            {
                _logger.LogInformation("No test orders need to sync up test results.");
                return [];
            }

            var testOrderIds = testOrdersFiltered.Select(t => t.TestOrderId).ToList();
            var syncUpEvent = new TestResultsSyncUpCheckedAtMonitoringIntegrationEvent
            {
                TestOrderIds = testOrderIds,
                UpdatedBy = request.UpdateBy
            };
            await _eventPublisher.PublishAsync(syncUpEvent);

            _logger.LogInformation("Sent request sync up test results to Monitoring Service: 'EventId' {EventId}", syncUpEvent.EventId);

            return [.. testOrdersFiltered];
        }
    }
}
