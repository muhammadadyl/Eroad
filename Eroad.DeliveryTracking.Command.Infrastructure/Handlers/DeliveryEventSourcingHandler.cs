using Eroad.CQRS.Core.Config;
using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Producers;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using Microsoft.Extensions.Options;


namespace Eroad.DeliveryTracking.Command.Infrastructure.Handlers
{
    public class DeliveryEventSourcingHandler : IEventSourcingHandler<DeliveryAggregate>
    {
        private readonly IEventStore _eventStore;
        private readonly IEventProducer _eventProducer;
        private readonly string _kafkaTopic;

        public DeliveryEventSourcingHandler(IEventStore eventStore, IEventProducer eventProducer, IOptions<KafkaConfig> kafkaConfig)
        {
            _eventStore = eventStore;
            _eventProducer = eventProducer;
            _kafkaTopic = kafkaConfig.Value?.Topic ?? throw new ArgumentNullException(nameof(kafkaConfig), "Kafka topic configuration is missing");
        }

        public async Task<DeliveryAggregate> GetByIdAsync(Guid aggregateId)
        {
            var events = await _eventStore.GetEventsAsync(aggregateId);

            if (events == null || !events.Any())
                throw new AggregateNotFoundException($"Delivery aggregate with ID {aggregateId} not found.");

            var aggregate = new DeliveryAggregate();
            aggregate.ReplayEvents(events);
            aggregate.Version = events.Select(x => x.Version).Max();

            return aggregate;
        }

        public async Task RepublishEventsAsync()
        {
            var aggregateIds = await _eventStore.GetAggregateIdsByTypeAsync(nameof(DeliveryAggregate));

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

        public async Task SaveAsync(DeliveryAggregate aggregate)
        {
            await _eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUncommittedChanges(), aggregate.Version, nameof(DeliveryAggregate));
            aggregate.MarkChangesAsCommitted();
        }
    }
}
