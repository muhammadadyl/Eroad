using System.ComponentModel.DataAnnotations;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public class AddVehicleCommand
    {
        [Required(ErrorMessage = "Vehicle ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Registration is required")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Registration must be between 1 and 20 characters")]
        public string Registration { get; set; }

        [Required(ErrorMessage = "Vehicle type is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Vehicle type must be between 1 and 50 characters")]
        public string VehicleType { get; set; }
    }
}