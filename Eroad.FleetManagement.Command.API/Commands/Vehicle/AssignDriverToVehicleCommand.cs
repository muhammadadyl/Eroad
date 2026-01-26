using Eroad.CQRS.Core.Commands;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public record AssignDriverToVehicleCommand : BaseCommand
    {
        public Guid VehicleId { get; init; }
        public Guid DriverId { get; init; }
    }
}
