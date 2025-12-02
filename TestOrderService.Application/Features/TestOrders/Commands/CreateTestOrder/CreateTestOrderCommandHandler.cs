using Mapster;
using Microsoft.Extensions.Logging;
using TestOrderService.Application.IntegrationEvents.TestOrders;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.Message;
using Entities=TestOrderService.Domain.Entities;

namespace TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder
{
    /// <summary>
    ///     Command handler for create test order implement.
    /// </summary>
    /// <seealso
    ///     cref="TestOrderService.Application.Interfaces.Message.ICommandHandler&lt;TestOrderService.Application.Features.TestOrders.Commands.CreateTestOrder.CreateTestOrderCommand, TestOrderService.Domain.Entities.TestOrder&gt;" />
    /// <seealso cref="ICommandHandler{CreateTestOrderCommand,Entities}.TestOrder&gt;" />
    public class CreateTestOrderCommandHandler(
        ITestOrderRepository testOrderRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<CreateTestOrderCommandHandler> logger) : ICommandHandler<CreateTestOrderCommand, Entities.TestOrder>
    {

        /// <summary>
        ///     The event publisher
        /// </summary>
        private readonly IEventPublisher _eventPublisher = eventPublisher;

        /// <summary>
        ///     The logger
        /// </summary>
        private readonly ILogger<CreateTestOrderCommandHandler> _logger = logger;
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
        public async Task<Entities.TestOrder> Handle(CreateTestOrderCommand request, CancellationToken cancellationToken)
        {
            var testOrder = request.Adapt<Entities.TestOrder>();

            var createdTestOrder = await _testOrderRepository.CreateTestOrderAsync(testOrder, cancellationToken);

            var createdTestOrderEvent = createdTestOrder.Adapt<TestOrderCreatedIntegrationEvent>();
            await _eventPublisher.PublishAsync(createdTestOrderEvent);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created test order with id: 'TestOrderId' {TestOrderId}", createdTestOrder.TestOrderId);

            return createdTestOrder;
        }
    }
}
