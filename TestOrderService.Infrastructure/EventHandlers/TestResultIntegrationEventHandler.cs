using Mapster;
using Microsoft.Extensions.Logging;
using TestOrderService.Application.IntegrationEvents;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Infrastructure.EventHandlers
{
    public class TestResultIntegrationEventHandler(
        ITestResultRepository testResultRepository,
        ILogger<TestResultIntegrationEventHandler> logger,
        IUnitOfWork unitOfWork,
        ITestOrderRepository testOrderRepository)
        : INotificationEventHandler<TestResultsCreatedIntegrationEvent>
    {

        private readonly ILogger<TestResultIntegrationEventHandler> _logger = logger;

        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;

        private readonly ITestResultRepository _testResultRepository = testResultRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(TestResultsCreatedIntegrationEvent request, CancellationToken cancellationToken)
        {
            var testOrderId = request.TestOrderId;
            var createdById = request.CreatedBy;
            _logger.LogInformation("Handling test results created event: 'TestOrderId' {TestOrderId}", testOrderId);

            var testResults = request.TestResults.Select(t => t.Adapt<TestResult>());

            foreach (var testResult in testResults)
            {
                await _testResultRepository.CreateTestResult(testResult, cancellationToken);
            }

            await _testOrderRepository.UpdateTestOrderToCompleted(testOrderId, createdById, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Handled test results created event successfully: 'TestOrderId' {TestOrderId}", testOrderId);
        }
    }
}
