using Eroad.CQRS.Core.Commands;
using Eroad.RouteManagement.Common;
using System.ComponentModel.DataAnnotations;

namespace Eroad.RouteManagement.Command.API.Commands.Route
{
    public record ChangeRouteStatusCommand : BaseCommand
    {
        [Required(ErrorMessage = "Old status is required")]
        public RouteStatus OldStatus { get; init; }

        [Required(ErrorMessage = "New status is required")]
        public RouteStatus NewStatus { get; init; }
    }
}
