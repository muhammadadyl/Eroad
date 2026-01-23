using Eroad.CQRS.Core.Domain;
using Eroad.CQRS.Core.Events;

namespace Eroad.CQRS.Core.Infrastructure
{
    public interface IEventStore
    {
        Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events, int expectedVersion, string aggregateType);
        Task<List<DomainEvent>> GetEventsAsync(Guid aggregateId);
        Task<List<Guid>> GetAggregateIdsByTypeAsync(string aggregateType);
    }
}