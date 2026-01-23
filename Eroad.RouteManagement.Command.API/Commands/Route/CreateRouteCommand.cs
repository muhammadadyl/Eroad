using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.RouteManagement.Command.API.Commands.Route
{
    public record CreateRouteCommand : BaseCommand
    {
        [Required(ErrorMessage = "Origin is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Origin must be between 1 and 200 characters")]
        public string Origin { get; init; }

        [Required(ErrorMessage = "Destination is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Destination must be between 1 and 200 characters")]
        public string Destination { get; init; }

        public Guid AssignedDriverId { get; init; }
        public Guid AssignedVehicleId { get; init; }
    }
}
