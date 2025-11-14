using MediatR;
namespace TestOrderService.Application.Interfaces.Message
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
