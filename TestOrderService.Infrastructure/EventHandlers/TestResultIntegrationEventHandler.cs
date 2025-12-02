using Mapster;
using Microsoft.Extensions.Logging;
using TestOrderService.Application.IntegrationEvents.TestResultIntegrationEvents;
using TestOrderService.Application.IntegrationEvents.TestResults;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Infrastructure.EventHandlers
{
    public class TestResultsCreatedIntegrationEventHandler(
        ITestResultRepository testResultRepository,
        ILogger<TestResultsCreatedIntegrationEventHandler> logger,
        IUnitOfWork unitOfWork,
        ITestOrderRepository testOrderRepository)
        : INotificationEventHandler<TestResultsCreatedIntegrationEvent>
    {

        private readonly ILogger<TestResultsCreatedIntegrationEventHandler> _logger = logger;

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
                await _testResultRepository.CreateTestResultAsync(testResult, cancellationToken);
            }

            await _testOrderRepository.UpdateTestOrderToCompleted(testOrderId, createdById, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Handled test results created event successfully: 'TestOrderId' {TestOrderId}", testOrderId);
        }
    }

    public class TestResultsSyncUpUpdatedFromMonitoringServiceIntegrationEventHandler(
        ILogger<TestResultsSyncUpUpdatedFromMonitoringServiceIntegrationEventHandler> logger,
        ITestResultRepository testResultRepository,
        ITestOrderRepository testOrderRepository,
        IUnitOfWork unitOfWork) :
        INotificationEventHandler<TestResultsSyncUpUpdatedFromMonitoringServiceIntegrationEvent>
    {
        private readonly ILogger<TestResultsSyncUpUpdatedFromMonitoringServiceIntegrationEventHandler> _logger = logger;

        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;

        private readonly ITestResultRepository _testResultRepository = testResultRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(TestResultsSyncUpUpdatedFromMonitoringServiceIntegrationEvent notification, CancellationToken cancellationToken)
        {
            var testOrderId = notification.TestOrderId;

            _logger.LogInformation("Handling test results sync up updated from 'Monitoring Service' integration event: 'TestOrderId' {TestOrderId}", testOrderId);

            var testResults = notification.TestResults;
            foreach (var testResult in testResults)
            {
                await _testResultRepository.CreateTestResultAsync(testResult, cancellationToken);
            }

            // Updating UpdatedBy and UpdatedAt field.
            var testOrder = await _testOrderRepository.GetTestOrderByIdWithTrackingAsync(testOrderId, cancellationToken)
                         ?? throw new InvalidDataException($"Can not find test order with 'TestOrderId': {testOrderId}.");
            testOrder.UpdateById = notification.UpdatedBy;
            testOrder.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Handled test results sync up updated from 'Monitoring Service' integration event successfully: 'TestOrderId' {TestOrderId}", testOrderId);
        }
    }

    public class TestResultsSyncUpUpdatedFromInstrumentIntegrationEventHandler(
        ILogger<TestResultsSyncUpUpdatedFromInstrumentIntegrationEventHandler> logger,
        ITestResultRepository testResultRepository,
        ITestOrderRepository testOrderRepository,
        IUnitOfWork unitOfWork)
        : INotificationEventHandler<TestResultsSyncUpUpdatedFromInstrumentIntegrationEvent>
    {
        private readonly ILogger<TestResultsSyncUpUpdatedFromInstrumentIntegrationEventHandler> _logger = logger;

        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;

        private readonly ITestResultRepository _testResultRepository = testResultRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task Handle(TestResultsSyncUpUpdatedFromInstrumentIntegrationEvent notification, CancellationToken cancellationToken)
        {
            var testOrderId = notification.TestOrderId;

            _logger.LogInformation("Handling test results sync up updated from 'Instrument Service' integration event: 'TestOrderId' {TestOrderId}", testOrderId);

            var testResults = notification.TestResults;
            foreach (var testResult in testResults)
            {
                await _testResultRepository.CreateTestResultAsync(testResult, cancellationToken);
            }

            var testOrder = await _testOrderRepository.GetTestOrderByIdWithTrackingAsync(testOrderId, cancellationToken)
                         ?? throw new InvalidDataException($"Can not find test order with 'TestOrderId': {testOrderId}.");
            testOrder.UpdateById = notification.UpdatedBy;
            testOrder.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Handled test results sync up updated from 'Instrument Service' integration event successfully: 'TestOrderId' {TestOrderId}", testOrderId);
        }
    }
}
