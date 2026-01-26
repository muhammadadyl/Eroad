using Eroad.CQRS.Core.Commands;
using Eroad.RouteManagement.Common;
using System.ComponentModel.DataAnnotations;

namespace Eroad.RouteManagement.Command.API.Commands.Route
{
    public record UpdateCheckpointCommand : BaseCommand
    {
        [Required(ErrorMessage = "Checkpoint is required")]
        public Checkpoint Checkpoint { get; init; }
    }
}
