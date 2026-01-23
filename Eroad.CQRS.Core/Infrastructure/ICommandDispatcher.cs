using Eroad.CQRS.Core.Commands;

namespace Eroad.CQRS.Core.Infrastructure
{
    public interface ICommandDispatcher
    {
        void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand;
        Task SendAsync(BaseCommand command);
    }
}