using Microsoft.Extensions.Logging;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.IntegrationEvents;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.Message;
using Entities=TestOrderService.Domain.Entities;

namespace TestOrderService.Application.Features.TestOrders.Commands.UpdateTestOrder
{
    /// <summary>
    ///     Command handler for update test order implement.
    /// </summary>
    /// <seealso cref="ICommandHandler{UpdateTestOrderCommand,Entities}.TestOrder}" />
    public class UpdateTestOrderCommandHandler(
        ITestOrderRepository testOrderRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<UpdateTestOrderCommandHandler> logger) : ICommandHandler<UpdateTestOrderCommand, Entities.TestOrder>
    {

        /// <summary>
        ///     The event publisher
        /// </summary>
        private readonly IEventPublisher _eventPublisher = eventPublisher;

        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger<UpdateTestOrderCommandHandler> _logger = logger;
        /// <summary>
        ///     The test order repository
        /// </summary>
        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;

        /// <summary>
        ///     The unit of work
        /// </summary>
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        ///     Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Entities.TestOrder> Handle(UpdateTestOrderCommand request, CancellationToken cancellationToken)
        {
            var testOrder = await _testOrderRepository.GetByIdAsync(request.TestOrderId, cancellationToken);

            if (testOrder == null)
            {
                throw new NotFoundException(
                    "Test order not found",
                    $"TestOrderId: {request.TestOrderId}"
                );
            }

            // Update only provided fields (partial update)
            // RunById and RunAt can be updated independently
            if (request.RunById.HasValue)
            {
                testOrder.RunById = request.RunById.Value;
            }

            if (request.RunAt.HasValue)
            {
                testOrder.RunAt = request.RunAt;
            }

            // TestOrderDescription can be set to null (empty string from frontend becomes null)
            if (request.TestOrderDescription != null)
            {
                // Allow empty string to clear the description
                testOrder.TestOrderDescription = string.IsNullOrWhiteSpace(request.TestOrderDescription)
                    ? null
                    : request.TestOrderDescription;
            }

            if (request.Status.HasValue)
            {
                testOrder.Status = request.Status.Value;
            }

            // Always set update tracking fields when updating
            testOrder.UpdateById = request.UpdateById;
            testOrder.UpdateAt = request.UpdateAt;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publish integration event
            var updatedEvent = new TestOrderUpdatedIntegrationEvent
            {
                TestOrderId = testOrder.TestOrderId,
                PatientId = testOrder.PatientId,
                Status = testOrder.Status.ToString(),
                RunById = testOrder.RunById,
                RunAt = testOrder.RunAt
            };
            await _eventPublisher.PublishAsync(updatedEvent);

            _logger.LogInformation("Updated test order with id: {TestOrderId}", testOrder.TestOrderId);

            return testOrder;
        }
    }
}
