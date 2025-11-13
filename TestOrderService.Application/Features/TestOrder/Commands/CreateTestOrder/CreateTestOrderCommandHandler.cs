using Mapster;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.Message;
using Entities=TestOrderService.Domain.Entities;

namespace TestOrderService.Application.Features.TestOrder.Commands.CreateTestOrder
{
    /// <summary>
    ///     Command handler for create test order implement.
    /// </summary>
    /// <seealso
    ///     cref="TestOrderService.Application.Interfaces.Message.ICommandHandler&lt;TestOrderService.Application.Features.TestOrder.Commands.CreateTestOrder.CreateTestOrderCommand, TestOrderService.Domain.Entities.TestOrder&gt;" />
    public class CreateTestOrderCommandHandler(
        ITestOrderRepository testOrderRepository,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateTestOrderCommand, Entities.TestOrder>
    {
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return createdTestOrder;
        }
    }
}
