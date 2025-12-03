using Mapster;
using Microsoft.Extensions.Logging;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.IntegrationEvents.TestOrders;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder
{
    public class UpdateTestOrderCommandHandler(
        ITestOrderRepository testOrderRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<UpdateTestOrderCommandHandler> logger)
        : ICommandHandler<UpdateTestOrderCommand, TestOrderDto>
    {
        private readonly IEventPublisher _eventPublisher = eventPublisher;
        private readonly ILogger<UpdateTestOrderCommandHandler> _logger = logger;
        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<TestOrderDto> Handle(UpdateTestOrderCommand request, CancellationToken cancellationToken)
        {
            var testOrder = await _testOrderRepository.GetByIdAsync(request.TestOrderId, cancellationToken);

            if (testOrder == null)
            {
                throw new NotFoundException(
                    "Test order not found",
                    $"TestOrderId: {request.TestOrderId}"
                );
            }

            // Partial update
            if (request.RunById.HasValue)
                testOrder.RunById = request.RunById.Value;

            if (request.RunAt.HasValue)
                testOrder.RunAt = request.RunAt;

            if (request.ReviewId.HasValue)
                testOrder.ReviewId = request.ReviewId;

            if (request.ReviewAt.HasValue)
                testOrder.ReviewAt = request.ReviewAt;

            if (request.TestOrderDescription != null)
            {
                testOrder.TestOrderDescription =
                    string.IsNullOrWhiteSpace(request.TestOrderDescription)
                        ? null
                        : request.TestOrderDescription;
            }

            if (request.Status.HasValue)
            {
                testOrder.Status = request.Status.Value;
                // If marking as Completed or Reviewed, ensure Review fields are set
                if (testOrder.Status == StatusTestOrder.Completed || testOrder.Status == StatusTestOrder.Reviewed)
                {
                    if (!testOrder.ReviewId.HasValue)
                        testOrder.ReviewId = request.ReviewId ?? request.UpdateById;
                    
                    if (!testOrder.ReviewAt.HasValue)
                        testOrder.ReviewAt = request.ReviewAt ?? DateTime.UtcNow;
                }
            }

            if (request.AIReviewSummary != null)
            {
                testOrder.AIReviewSummary = request.AIReviewSummary;
                _logger.LogInformation("AIReviewSummary updated for TestOrder {TestOrderId}", testOrder.TestOrderId);
            }

            if (request.Status == StatusTestOrder.AIReviewed)
            {
                foreach (var tr in testOrder.TestResults)
                {
                    tr.Status = TestResultStatus.AIReviewed;
                    tr.ReviewedBy = request.UpdateById;
                    tr.ReviewedAt = DateTime.UtcNow;
                }

                _logger.LogInformation(
                    "Auto updated {Count} TestResults to AIReviewed for TestOrder {TestOrderId}",
                    testOrder.TestResults.Count,
                    testOrder.TestOrderId
                );
            }

            testOrder.UpdateById = request.UpdateById;
            testOrder.UpdateAt = request.UpdateAt;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publish event
            var updatedEvent = new TestOrderUpdatedIntegrationEvent
            {
                TestOrderId = testOrder.TestOrderId,
                PatientId = testOrder.PatientId,
                Status = testOrder.Status.ToString(),
                RunById = testOrder.RunById,
                RunAt = testOrder.RunAt
            };
            await _eventPublisher.PublishAsync(updatedEvent);

            return testOrder.Adapt<TestOrderDto>();
        }
    }
}
