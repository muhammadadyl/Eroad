using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record UpdateCurrentCheckpointCommand : BaseCommand
    {
        [Required(ErrorMessage = "Route ID is required")]
        public Guid RouteId { get; init; }

        [Required(ErrorMessage = "Sequence is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Sequence must be greater than 0")]
        public int Sequence { get; init; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Location must be between 1 and 200 characters")]
        public string Location { get; init; } = string.Empty;
    }
}
