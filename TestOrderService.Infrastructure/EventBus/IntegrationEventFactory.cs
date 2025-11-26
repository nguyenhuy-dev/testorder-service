using System.Text.Json;
using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Infrastructure.EventBus
{
    public class IntegrationEventFactory : IIntegrationEventFactory
    {
        public static readonly IntegrationEventFactory Instance = new IntegrationEventFactory();

        public IntegrationEvent? CreateEvent(string typeName, string value)
        {
            var t = GetEventType(typeName) ?? throw new InvalidDataException($"Type {typeName} not found.");

            return JsonSerializer.Deserialize(value, t) as IntegrationEvent;
        }

        private static Type? GetEventType(string typeName)
        {
            var t = Type.GetType(typeName);

            return t ?? AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    {
                        var lastDotT = t.FullName!.LastIndexOf('.');
                        var tName = t.FullName.Substring(lastDotT + 1);

                        var lastDotType = typeName.LastIndexOf('.');
                        var typeOnlyName = typeName.Substring(lastDotType + 1);
                        return tName == typeOnlyName;
                    }
                );
        }
    }
}
