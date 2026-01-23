using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Events;
using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.CQRS.Core.Producers;
using Eroad.DeliveryTracking.Command.Infrastructure.Config;
using Microsoft.Extensions.Options;

namespace Eroad.DeliveryTracking.Command.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly IEventProducer _eventProducer;
        private readonly string _kafkaTopic;

        public EventStore(IEventStoreRepository eventStoreRepository, IEventProducer eventProducer, IOptions<KafkaConfig> kafkaConfig)
        {
            _eventStoreRepository = eventStoreRepository;
            _eventProducer = eventProducer;
            _kafkaTopic = kafkaConfig.Value?.Topic ?? throw new ArgumentNullException(nameof(kafkaConfig), "Kafka topic configuration is missing");
        }

        public async Task<List<Guid>> GetAggregateIdsAsync()
        {
            var eventStream = await _eventStoreRepository.FindAllAsync();

            if (eventStream == null || !eventStream.Any())
                throw new ArgumentNullException(nameof(eventStream), "Could not retrieve event stream from the event store!");

            return eventStream.Select(x => x.AggregateIdentifier).Distinct().ToList();
        }

        public async Task<List<Guid>> GetAggregateIdsByTypeAsync(string aggregateType)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateType(aggregateType);

            if (eventStream == null || !eventStream.Any())
                return new List<Guid>();

            return eventStream.Select(x => x.AggregateIdentifier).Distinct().ToList();
        }

        public async Task<List<DomainEvent>> GetEventsAsync(Guid aggregateId)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);

            if (eventStream == null || !eventStream.Any())
                throw new AggregateNotFoundException("Incorrect delivery ID provided!");

            return eventStream.OrderBy(x => x.Version).Select(x => x.EventData).ToList();
        }

        public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events, int expectedVersion, string aggregateType)
        {
            var eventStream = await _eventStoreRepository.FindByAggregateId(aggregateId);

            if (expectedVersion != -1 && eventStream != null && eventStream.Any())
            {
                if (eventStream[^1].Version != expectedVersion)
                    throw new ConcurrencyException();
            }

            var version = expectedVersion;

            foreach (var @event in events)
            {
                version++;
                @event.Version = version;
                var eventType = @event.GetType().Name;
                var eventModel = new EventModel
                {
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = aggregateType,
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await _eventStoreRepository.SaveAsync(eventModel);

                await _eventProducer.ProduceAsync(_kafkaTopic, @event);
            }
        }
    }
}
