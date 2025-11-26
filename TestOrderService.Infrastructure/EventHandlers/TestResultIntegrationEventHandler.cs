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
        IUnitOfWork unitOfWork)
        : INotificationEventHandler<TestResultCreatedIntegrationEvent>
    {

        private readonly ILogger<TestResultIntegrationEventHandler> _logger = logger;
        private readonly ITestResultRepository _testResultRepository = testResultRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(TestResultCreatedIntegrationEvent request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling test result created event: 'TestResultId' {TestResultId}", request.TestResultId);

            var testResult = request.Adapt<TestResult>();

            await _testResultRepository.CreateTestResult(testResult, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Handling for creating test result successfully: 'TestResultId' {TestResultId}", request.TestResultId);
        }
    }
}
