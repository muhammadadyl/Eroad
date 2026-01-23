namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public class AssignDriverToVehicleCommand
    {
        public Guid VehicleId { get; set; }
        public Guid DriverId { get; set; }
    }
}
