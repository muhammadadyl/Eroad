using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public class ChangeVehicleStatusCommand
    {
        public Guid Id { get; set; }
        public VehicleStatus OldStatus { get; set; }
        public VehicleStatus NewStatus { get; set; }
        public string Reason { get; set; }
    }
}