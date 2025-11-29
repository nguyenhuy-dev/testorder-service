using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetTestOrderById
{
    public sealed record GetTestOrderByIdQuery(Guid TestOrderId) : IQuery<TestOrder>;
}
