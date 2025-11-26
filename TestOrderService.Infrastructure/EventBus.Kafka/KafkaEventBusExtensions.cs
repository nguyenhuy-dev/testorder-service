using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TestOrderService.Application.Interfaces.EventBus;
namespace TestOrderService.Infrastructure.EventBus.Kafka
{
    /// <summary>
    ///     Register kafka producer into the DI container.
    /// </summary>
    public static class KafkaEventBusExtensions
    {
        /// <summary>
        ///     Adds the kafka producer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="connectionName">Name of the connection.</param>
        /// <returns></returns>
        public static IHostApplicationBuilder AddKafkaProducer(this IHostApplicationBuilder builder, string connectionName)
        {
            builder.AddKafkaProducer<string, MessageEnvelop>(connectionName, settings => { },
                builder =>
                {
                    builder.SetValueSerializer(new MessageEnvelopSerializer());
                }
            );

            return builder;
        }

        /// <summary>
        ///     Adds the kafka event publisher.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="topic">The topic.</param>
        /// <exception cref="System.ArgumentNullException">topic</exception>
        public static void AddKafkaEventPublisher(this IHostApplicationBuilder builder, string? topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentNullException(nameof(topic));

            builder.Services.AddTransient<IEventPublisher>(services => new KafkaEventPublisher(topic,
                services.GetRequiredService<IProducer<string, MessageEnvelop>>(),
                services.GetRequiredService<ILoggerFactory>().CreateLogger($"EventPublisher<{topic}>"))
            );
        }

        public static IHostApplicationBuilder AddKafkaMessageEnvelopConsumer(this IHostApplicationBuilder builder, string groupId, string connectionName = "kafka")
        {
            builder.AddKafkaConsumer<string, MessageEnvelop>(connectionName,
                settings =>
                {
                    settings.Config.GroupId = groupId;
                    settings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
                },
                builder =>
                {
                    builder.SetValueDeserializer(new MessageEnvelopDeserializer());
                }
            );

            return builder;
        }

        public static IHostApplicationBuilder AddKafkaEventConsumer(this IHostApplicationBuilder builder, Action<EventHandlingWorkerOptions>? configureOptions = null, string connectionName = "kafka")
        {
            var options = new EventHandlingWorkerOptions();
            configureOptions?.Invoke(options);

            builder.AddKafkaMessageEnvelopConsumer(options.KafkaGroupId, connectionName);
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton(services => options.IntegrationEventFactory);
            builder.Services.AddHostedService<EventHandlingService>();

            return builder;
        }
    }

    /// <summary>
    /// </summary>
    /// <seealso cref="Confluent.Kafka.ISerializer&lt;TestOrderService.Application.Interfaces.EventBus.MessageEnvelop&gt;" />
    internal class MessageEnvelopSerializer : ISerializer<MessageEnvelop>
    {
        /// <summary>
        ///     Serialize the key or value of a <see cref="T:Confluent.Kafka.Message`2" />
        ///     instance.
        /// </summary>
        /// <param name="data">The value to serialize.</param>
        /// <param name="context">Context relevant to the serialize operation.</param>
        /// <returns>
        ///     The serialized value.
        /// </returns>
        public byte[] Serialize(MessageEnvelop data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }

    internal class MessageEnvelopDeserializer : IDeserializer<MessageEnvelop>
    {
        public MessageEnvelop Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<MessageEnvelop>(data) ?? throw new InvalidOperationException("Error deserialize data.");
        }
    }
}
