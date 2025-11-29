using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestOrderService.Application.Interfaces.EventBus;
using TestOrderService.Application.Interfaces.Events;
namespace TestOrderService.Infrastructure.EventBus.Kafka
{
    public class EventHandlingService(
        IConsumer<string, MessageEnvelop> consumer,
        EventHandlingWorkerOptions options,
        IIntegrationEventFactory integrationEventFactory,
        IServiceScopeFactory serviceScopeFactory,
        ILoggerFactory loggerFactory) : BackgroundService
    {
        private readonly IConsumer<string, MessageEnvelop> _consumer = consumer;

        private readonly IIntegrationEventFactory _integrationEventFactory = integrationEventFactory;

        private readonly ILogger _logger = loggerFactory.CreateLogger(options.ServiceName);

        private readonly EventHandlingWorkerOptions _options = options;

        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subcribing to topics [{Topics}]...", string.Join(',', _options.Topics));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _consumer.Subscribe(_options.Topics);

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = _consumer.Consume(100);

                            if (consumeResult != null)
                            {
                                using var scope = _serviceScopeFactory.CreateScope();
                                var publisherMediator = scope.ServiceProvider.GetRequiredService<IPublisher>();
                                await ProcessMessageAsync(publisherMediator, consumeResult.Message.Value, stoppingToken);
                            }
                            else
                            {
                                await Task.Delay(100, stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error consuming message.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error subcribing to topics.");
                }

                await Task.Delay(100, stoppingToken);
            }
        }

        private async Task ProcessMessageAsync(IPublisher publisherMediator, MessageEnvelop message, CancellationToken stoppingToken)
        {
            var @event = _integrationEventFactory.CreateEvent(message.MessageTypeName, message.Message);

            if (@event is not null)
            {
                if (_options.AcceptEvent(@event))
                {
                    _logger.LogInformation("Processing message {T}: {Message}", message.MessageTypeName, message.Message);

                    await publisherMediator.Publish(@event, stoppingToken);
                }
                else
                    _logger.LogDebug("Event skipped: {T}", message.MessageTypeName);
            }
            else
                _logger.LogWarning("Event type not found: {T}", message.MessageTypeName);
        }
    }

    public class EventHandlingWorkerOptions
    {
        public string KafkaGroupId { get; set; } = "event-handling";

        public List<string> Topics { get; set; } = [];

        public IIntegrationEventFactory IntegrationEventFactory { get; set; } = EventBus.IntegrationEventFactory.Instance;

        public string ServiceName { get; set; } = "EventHandlingService";

        public Func<IntegrationEvent, bool> AcceptEvent { get; set; } = _ => true;
    }
}
