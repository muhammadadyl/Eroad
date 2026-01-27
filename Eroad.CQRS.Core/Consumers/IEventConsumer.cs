namespace Eroad.CQRS.Core.Consumers
{
    public interface IEventConsumer
    {
        Task ConsumeAsync(string topic, CancellationToken cancellationToken);
    }
}