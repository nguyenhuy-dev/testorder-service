using MediatR;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Interfaces;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetTestOrders
{
    public class GetTestOrdersQueryHandler : IRequestHandler<GetTestOrdersQuery, PaginatedList<TestOrderDto>>
    {
        private readonly ITestOrderRepository _testOrderRepository;

        public GetTestOrdersQueryHandler(ITestOrderRepository testOrderRepository)
        {
            _testOrderRepository = testOrderRepository;
        }

        /// <summary>
        ///     Handles the GetTestOrdersQuery request
        /// </summary>
        /// <param name="request">Query containing request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of test orders</returns>
        public async Task<PaginatedList<TestOrderDto>> Handle(GetTestOrdersQuery request, CancellationToken cancellationToken)
        {
            return await _testOrderRepository.GetTestOrdersAsync(request.Request, cancellationToken);
        }
    }
}
