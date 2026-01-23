using Eroad.RouteManagement.Command.API.Commands.Route;

namespace Eroad.RouteManagement.Command.API.Commands
{
    public interface IRouteCommandHandler
    {
        Task HandleAsync(CreateRouteCommand command);
        Task HandleAsync(UpdateRouteCommand command);
        Task HandleAsync(ChangeRouteStatusCommand command);
        Task HandleAsync(AddCheckpointCommand command);
        Task HandleAsync(UpdateCheckpointCommand command);
        Task HandleAsync(AssignDriverToRouteCommand command);
        Task HandleAsync(AssignVehicleToRouteCommand command);
    }
}
