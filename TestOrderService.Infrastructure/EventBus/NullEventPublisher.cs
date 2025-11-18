using Microsoft.Extensions.Logging;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Infrastructure.EventBus
{
    /// <summary>
    ///     Null event publisher.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Interfaces.EventBus.IEventPublisher" />
    public class NullEventPublisher : IEventPublisher
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NullEventPublisher" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public NullEventPublisher(ILogger<NullEventPublisher> logger)
        {
            logger.LogInformation("NullEventPublisher is used.");
        }

        /// <summary>
        ///     Publishes the asynchronous.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public Task<bool> PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
        {
            return Task.FromResult(true);
        }
    }
}
