using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetTestOrderById
{
    public class GetTestOrderByIdQueryHandler(ITestOrderRepository testOrderRepository) : IQueryHandler<GetTestOrderByIdQuery, TestOrder>
    {
        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;

        public async Task<TestOrder> Handle(GetTestOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _testOrderRepository.GetTestOrderByIdNotIncludeAsync(request.TestOrderId, cancellationToken)
                ?? throw new NotFoundException("Test order does not exist: 'TestOrderId' {TestOrderId}.", request.TestOrderId);
        }
    }
}
