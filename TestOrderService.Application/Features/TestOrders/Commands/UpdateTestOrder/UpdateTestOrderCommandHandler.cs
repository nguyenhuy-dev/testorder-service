using Mapster;
using Microsoft.Extensions.Logging;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.IntegrationEvents;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.Message;
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

            if (request.TestOrderDescription != null)
            {
                testOrder.TestOrderDescription =
                    string.IsNullOrWhiteSpace(request.TestOrderDescription)
                        ? null
                        : request.TestOrderDescription;
            }

            if (request.Status.HasValue)
                testOrder.Status = request.Status.Value;

            if (request.AIReviewSummary != null)
            {
                testOrder.AIReviewSummary = request.AIReviewSummary;
                _logger.LogInformation("AIReviewSummary updated for TestOrder {TestOrderId}", testOrder.TestOrderId);
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
