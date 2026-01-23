using Eroad.CQRS.Core.Handlers;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Producers;
using Eroad.FleetManagement.Command.Domain.Aggregates;

namespace Eroad.FleetMangement.Command.Infrastructure.Handlers
{
    public class VehicleEventSourcingHandler : IEventSourcingHandler<VehicleAggregate>
    {
        private readonly IEventStore _eventStore;
        private readonly IEventProducer _eventProducer;

        public VehicleEventSourcingHandler(IEventStore eventStore, IEventProducer eventProducer)
        {
            _eventStore = eventStore;
            _eventProducer = eventProducer;
        }

        public async Task<VehicleAggregate> GetByIdAsync(Guid aggregateId)
        {
            var aggregate = new VehicleAggregate();
            var events = await _eventStore.GetEventsAsync(aggregateId);

            if (events == null || !events.Any()) return aggregate;

            aggregate.ReplayEvents(events);
            aggregate.Version = events.Select(x => x.Version).Max();

            return aggregate;
        }

        public async Task RepublishEventsAsync()
        {
            var aggregateIds = await _eventStore.GetAggregateIdsByTypeAsync(nameof(VehicleAggregate));

            if (aggregateIds == null || !aggregateIds.Any()) return;

            foreach (var aggregateId in aggregateIds)
            {
                var events = await _eventStore.GetEventsAsync(aggregateId);

                foreach (var @event in events)
                {
                    var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
                    await _eventProducer.ProduceAsync(topic, @event);
                }
            }
        }

        public async Task SaveAsync(VehicleAggregate aggregate)
        {
            await _eventStore.SaveEventsAsync(aggregate.Id, aggregate.GetUncommittedChanges(), aggregate.Version, nameof(VehicleAggregate));
            aggregate.MarkChangesAsCommitted();
        }
    }
}