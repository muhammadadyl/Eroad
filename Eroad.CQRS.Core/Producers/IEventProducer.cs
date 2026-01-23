using Eroad.CQRS.Core.Events;

namespace Eroad.CQRS.Core.Producers
{
    public interface IEventProducer
    {
        Task ProduceAsync<T>(string topic, T @event) where T : DomainEvent;
    }
}