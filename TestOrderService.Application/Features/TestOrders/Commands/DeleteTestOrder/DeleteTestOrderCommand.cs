using MediatR;
namespace TestOrderService.Application.Features.TestOrders.Commands.DeleteTestOrder
{
    public record DeleteTestOrderCommand(Guid TestOrderId) : IRequest<bool>;

}
