using System.Text.Json;
using Confluent.Kafka;
using Eroad.CQRS.Core.Consumers;
using Eroad.CQRS.Core.Events;
using Eroad.RouteManagement.Query.Infrastructure.Converters;
using Eroad.RouteManagement.Query.Infrastructure.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eroad.RouteManagement.Query.Infrastructure.Consumers
{
    public class EventConsumer : IEventConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly IEventHandler _eventHandler;
        private readonly ILogger<EventConsumer> _logger;

        public EventConsumer(IOptions<ConsumerConfig> config, IEventHandler eventHandler, ILogger<EventConsumer> logger)
        {
            _config = config.Value;
            _eventHandler = eventHandler;
            _logger = logger;
        }

        public async Task ConsumeAsync(string topic, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting to consume events from topic: {Topic}", topic);
            
            int retryCount = 0;
            const int maxRetries = 5;
            const int retryDelayMs = 2000;

            using var consumer = new ConsumerBuilder<string, string>(_config)
                    .SetKeyDeserializer(Deserializers.Utf8)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .Build();

            consumer.Subscribe(topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);

                        if (consumeResult?.Message == null) continue;

                        retryCount = 0; // Reset retry count on successful consume

                        try
                        {
                            _logger.LogDebug("Received message: {Message}", consumeResult.Message.Value);
                            
                            var options = new JsonSerializerOptions { Converters = { new EventJsonConverter() } };
                            var @event = JsonSerializer.Deserialize<DomainEvent>(consumeResult.Message.Value, options);
                            
                            if (@event == null)
                            {
                                _logger.LogError("Failed to deserialize event from message: {Message}", consumeResult.Message.Value);
                                consumer.Commit(consumeResult);
                                continue;
                            }

                            _logger.LogInformation("Deserialized event of type: {EventType}", @event.GetType().Name);
                            
                            var handlerMethod = _eventHandler.GetType().GetMethod("On", new Type[] { @event.GetType() });

                            if (handlerMethod == null)
                            {
                                _logger.LogError("Could not find event handler method for event type: {EventType}", @event.GetType().Name);
                                consumer.Commit(consumeResult);
                                continue;
                            }

                            _logger.LogInformation("Invoking handler for event type: {EventType}", @event.GetType().Name);
                            var task = (Task)handlerMethod.Invoke(_eventHandler, new object[] { @event });
                            await task;
                            _logger.LogInformation("Successfully processed event of type: {EventType}", @event.GetType().Name);
                            
                            consumer.Commit(consumeResult);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message");
                            consumer.Commit(consumeResult);
                        }
                    }
                    catch (Confluent.Kafka.ConsumeException ex) when (ex.Error.Code == Confluent.Kafka.ErrorCode.UnknownTopicOrPart)
                    {
                        retryCount++;
                        if (retryCount <= maxRetries)
                        {
                            _logger.LogWarning("Topic '{Topic}' not available yet. Retrying in {DelayMs}ms ({RetryCount}/{MaxRetries})", 
                                topic, retryDelayMs, retryCount, maxRetries);
                            await Task.Delay(retryDelayMs, cancellationToken);
                        }
                        else
                        {
                            _logger.LogError("Topic '{Topic}' not available after {MaxRetries} retries. Topic may not be created.", topic, maxRetries);
                            throw;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Event consumer was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in event consumer");
            }
            finally
            {
                consumer.Unsubscribe();
                consumer.Close();
                _logger.LogInformation("Event consumer stopped");
            }
        }
    }
}
