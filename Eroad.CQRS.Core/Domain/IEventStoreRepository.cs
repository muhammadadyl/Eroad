using Eroad.CQRS.Core.Events;

namespace Eroad.CQRS.Core.Domain
{
    public interface IEventStoreRepository
    {
        Task SaveAsync(EventModel @event);
        Task<List<EventModel>> FindByAggregateId(Guid aggregateId);
        Task<List<EventModel>> FindAllAsync();
        Task<List<EventModel>> FindByAggregateType(string aggregateType);
    }
}