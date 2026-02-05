using System.Text.Json;
using Confluent.Kafka;
using Eroad.CQRS.Core.Infrastructure.Consumers;
using Eroad.RouteManagement.Query.Infrastructure.Converters;
using Eroad.RouteManagement.Query.Infrastructure.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eroad.RouteManagement.Query.Infrastructure.Consumers
{
    public class EventConsumer : GenericEventConsumer
    {
        public EventConsumer(
            IOptions<ConsumerConfig> config,
            IEventHandler eventHandler,
            ILogger<EventConsumer> logger)
            : base(config, eventHandler, logger)
        {
        }

        protected override void ConfigureJsonSerializerOptions(JsonSerializerOptions options)
        {
            options.Converters.Add(new EventJsonConverter());
        }
    }
}

