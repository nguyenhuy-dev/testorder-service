using MediatR;
using TestOrderService.Application.DTOs;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetDetail
{
    /// <summary>
    ///     Represents a query to retrieve detailed information for a specific TestOrder.
    /// </summary>
    public record GetTestOrderDetailQuery(Guid TestOrderId)
        : IRequest<TestOrderDetailDto>;

}
