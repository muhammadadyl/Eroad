namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public class AddVehicleCommand
    {
        public Guid Id { get; set; }
        public string Registration { get; set; }
        public string VehicleType { get; set; }
    }
}