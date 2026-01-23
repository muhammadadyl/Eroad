using Eroad.FleetManagement.Command.API.Commands.Vehicle;

namespace Eroad.FleetManagement.Command.API.Commands
{
    public interface IVehicleCommandHandler
    {
        Task HandleAsync(AddVehicleCommand command);
        Task HandleAsync(UpdateVehicleCommand command);
        Task HandleAsync(ChangeVehicleStatusCommand command);
        Task HandleAsync(AssignDriverToVehicleCommand command);

    }
}