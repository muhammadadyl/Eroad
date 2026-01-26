using Eroad.CQRS.Core.Config;
using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Events;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Eroad.CQRS.Core.Infrastructure.Repositories
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly IMongoCollection<EventModel> _eventStoreCollection;

        public EventStoreRepository(IOptions<MongoDbConfig> config)
        {
            var mongoClient = new MongoClient(config.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(config.Value.Database);

            _eventStoreCollection = mongoDatabase.GetCollection<EventModel>(config.Value.Collection);
        }

        public async Task<List<EventModel>> FindAllAsync()
        {
            return await _eventStoreCollection.Find(_ => true).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<EventModel>> FindByAggregateId(Guid aggregateId)
        {
            return await _eventStoreCollection.Find(x => x.AggregateIdentifier == aggregateId).ToListAsync().ConfigureAwait(false);
        }

        public async Task SaveAsync(EventModel @event)
        {
            await _eventStoreCollection.InsertOneAsync(@event).ConfigureAwait(false);
        }

        public async Task<List<EventModel>> FindByAggregateType(string aggregateType)
        {
            return await _eventStoreCollection.Find(x => x.AggregateType == aggregateType).ToListAsync().ConfigureAwait(false);
        }
    }
}
