using Eroad.CQRS.Core.Domain;

namespace Eroad.CQRS.Core.Handlers
{
    public interface IEventSourcingHandler<T> where T : AggregateRoot
    {
        Task SaveAsync(T aggregate);
        Task<T> GetByIdAsync(Guid aggregateId);
        Task RepublishEventsAsync();
    }
}