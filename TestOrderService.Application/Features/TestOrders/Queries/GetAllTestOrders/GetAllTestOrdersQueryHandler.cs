using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetAllTestOrders
{
    public class GetAllTestOrdersQueryHandler(ITestOrderRepository testOrderRepository) : IQueryHandler<GetAllTestOrdersQuery, List<TestOrder>>
    {
        private readonly ITestOrderRepository _testOrderRepository = testOrderRepository;

        public async Task<List<TestOrder>> Handle(GetAllTestOrdersQuery request, CancellationToken cancellationToken)
        {
            var testOrders = await _testOrderRepository.GetAllTestOrdersAsync(cancellationToken);

            return testOrders.ToList();
        }
    }
}
