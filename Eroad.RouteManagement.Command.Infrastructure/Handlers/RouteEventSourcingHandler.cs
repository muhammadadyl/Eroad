using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Producers;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using Eroad.RouteManagement.Command.Infrastructure.Config;
using Microsoft.Extensions.Options;


namespace Eroad.RouteManagement.Command.Infrastructure.Handlers
{
    public class RouteEventSourcingHandler : IEventSourcingHandler<RouteAggregate>
    {
        private readonly IEventStore _eventStore;
        private readonly IEventProducer _eventProducer;
        private readonly string _kafkaTopic;

        public RouteEventSourcingHandler(IEventStore eventStore, IEventProducer eventProducer, IOptions<KafkaConfig> kafkaConfig)
        {
            _eventStore = eventStore;
            _eventProducer = eventProducer;
            _kafkaTopic = kafkaConfig.Value?.Topic ?? throw new ArgumentNullException(nameof(kafkaConfig), "Kafka topic configuration is missing");
        }

        public async Task<RouteAggregate> GetByIdAsync(Guid aggregateId)
        {
            var events = await _eventStore.GetEventsAsync(aggregateId);

            if (events == null || !events.Any())
                throw new AggregateNotFoundException($"Route aggregate with ID {aggregateId} not found.");

            var aggregate = new RouteAggregate();
            aggregate.ReplayEvents(events);
            aggregate.Version = events.Select(x => x.Version).Max();

            return aggregate;
        }

        public async Task RepublishEventsAsync()
        {
            var aggregateIds = await _eventStore.GetAggregateIdsByTypeAsync(nameof(RouteAggregate));

            if (aggregateIds == null || !aggregateIds.Any()) return;

            foreach (var aggregateId in aggregateIds)
            {
                var events = await _eventStore.GetEventsAsync(aggregateId);

                foreach (var @event in events)
                {
                    await _eventProducer.ProduceAsync(_kafkaTopic, @event);
                }
            }
        }

        public async Task SaveAsync(RouteAggregate aggregate)
        {
            await _eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUncommittedChanges(), aggregate.Version, nameof(RouteAggregate));
            aggregate.MarkChangesAsCommitted();
        }
    }
}
