using MediatR;
using Microsoft.Extensions.Logging;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Commands
{
    public class DeleteTestOrderCommandHandler(
        ITestOrderRepository testOrderRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTestOrderCommandHandler> logger
    ) : IRequestHandler<DeleteTestOrderCommand, bool>
    {
        public async Task<bool> Handle(DeleteTestOrderCommand request, CancellationToken cancellationToken)
        {
            var testOrder = await testOrderRepository.GetByIdAsync(request.TestOrderId, cancellationToken);
            if (testOrder == null)
                throw new NotFoundException("Test order not found", $"TestOrderId: {request.TestOrderId}");

            if (testOrder.Status != StatusTestOrder.Completed)
                throw new BusinessRuleException(
                    "Cannot delete test order",
                    "Only completed test orders can be deleted."
                );

            testOrderRepository.Delete(testOrder);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Test order {Id} deleted at {Time}", testOrder.TestOrderId, DateTime.UtcNow);

            return true;
        }
    }
}
