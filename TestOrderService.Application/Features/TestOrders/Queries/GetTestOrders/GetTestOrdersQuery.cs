using MediatR;
using TestOrderService.Application.DTOs;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetTestOrders
{
    /// <summary>
    ///     Query to get paginated test orders with optional status filter
    /// </summary>
    public record GetTestOrdersQuery(GetTestOrdersRequest Request) : IRequest<PaginatedList<TestOrderDto>>;
}
