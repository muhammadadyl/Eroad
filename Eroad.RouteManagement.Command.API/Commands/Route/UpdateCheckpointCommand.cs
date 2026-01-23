using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.RouteManagement.Command.API.Commands.Route
{
    public record UpdateCheckpointCommand : BaseCommand
    {
        [Required(ErrorMessage = "Sequence is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Sequence must be a positive number")]
        public int Sequence { get; init; }

        public DateTime? ActualTime { get; init; }
    }
}
