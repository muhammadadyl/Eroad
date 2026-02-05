namespace Eroad.CQRS.Core.Infrastructure.Consumers
{
    public interface IEventConsumer
    {
        Task ConsumeAsync(string topic, CancellationToken cancellationToken);
    }
}