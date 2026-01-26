using Eroad.CQRS.Core.Commands;
using Eroad.FleetManagement.Common;
using System.ComponentModel.DataAnnotations;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public record ChangeVehicleStatusCommand : BaseCommand
    {
        [Required(ErrorMessage = "Old status is required")]
        public VehicleStatus OldStatus { get; init; }

        [Required(ErrorMessage = "New status is required")]
        public VehicleStatus NewStatus { get; init; }

        [StringLength(200, ErrorMessage = "Reason must not exceed 200 characters")]
        public string Reason { get; init; }
    }
}