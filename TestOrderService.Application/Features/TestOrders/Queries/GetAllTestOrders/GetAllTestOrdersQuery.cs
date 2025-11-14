using TestOrderService.Application.Interfaces.Message;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetAllTestOrders
{
    public sealed record GetAllTestOrdersQuery : IQuery<List<TestOrder>>;
}
