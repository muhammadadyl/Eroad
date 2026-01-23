using Eroad.FleetManagement.Command.API.Commands.Driver;

namespace Eroad.FleetManagement.Command.API.Commands
{
    public interface IDriverCommandHandler
    {
        Task HandleAsync(AddDriverCommand command);
        Task HandleAsync(UpdateDriverCommand command);
        Task HandleAsync(ChangeDriverStatusCommand command);
    }
}
