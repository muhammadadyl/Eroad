using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.RouteManagement.Command.API.Commands.Route
{
    public record AssignDriverToRouteCommand : BaseCommand
    {
        [Required(ErrorMessage = "Driver ID is required")]
        public Guid DriverId { get; init; }
    }
}
