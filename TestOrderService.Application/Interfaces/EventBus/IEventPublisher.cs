using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Application.Interfaces.EventBus
{
    /// <summary>
    ///     Interface for event publisher.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        ///     Publishes the asynchronous.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent;
    }
}
