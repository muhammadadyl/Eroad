using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.RouteManagement.Command.API.Commands.Route
{
    public record AssignVehicleToRouteCommand : BaseCommand
    {
        [Required(ErrorMessage = "Vehicle ID is required")]
        public Guid VehicleId { get; init; }
    }
}
