using MediatR;
namespace TestOrderService.Application.Interfaces.Message
{
    public interface INotificationEventHandler<in TNotification> : INotificationHandler<TNotification> where TNotification : INotification
    {
    }
}
