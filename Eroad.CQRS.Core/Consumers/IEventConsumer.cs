namespace Eroad.CQRS.Core.Consumers
{
    public interface IEventConsumer
    {
        void Consume(string topic);
    }
}