using Eroad.FleetManagement.Common;
using System.ComponentModel.DataAnnotations;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public class ChangeVehicleStatusCommand
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Old status is required")]
        public VehicleStatus OldStatus { get; set; }

        [Required(ErrorMessage = "New status is required")]
        public VehicleStatus NewStatus { get; set; }

        [StringLength(200, ErrorMessage = "Reason must not exceed 200 characters")]
        public string Reason { get; set; }
    }
}