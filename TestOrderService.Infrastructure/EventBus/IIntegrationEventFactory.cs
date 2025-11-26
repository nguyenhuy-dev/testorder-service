using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Infrastructure.EventBus
{
    /// <summary>
    ///     Interface for factory.
    /// </summary>
    public interface IIntegrationEventFactory
    {
        /// <summary>
        ///     Creates the event.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IntegrationEvent? CreateEvent(string typeName, string value);
    }
}
