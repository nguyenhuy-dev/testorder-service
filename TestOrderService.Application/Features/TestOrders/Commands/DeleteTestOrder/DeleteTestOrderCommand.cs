using MediatR;
namespace TestOrderService.Application.Features.TestOrders.Commands
{
    public record DeleteTestOrderCommand(Guid TestOrderId) : IRequest<bool>;

}
