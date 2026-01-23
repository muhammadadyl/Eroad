using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record UpdateCurrentCheckpointCommand : BaseCommand
    {
        [Required(ErrorMessage = "Checkpoint is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Checkpoint must be between 1 and 200 characters")]
        public string Checkpoint { get; init; }
    }
}
