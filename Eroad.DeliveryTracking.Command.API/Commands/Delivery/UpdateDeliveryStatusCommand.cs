using Eroad.CQRS.Core.Commands;
using Eroad.DeliveryTracking.Common;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record UpdateDeliveryStatusCommand : BaseCommand
    {
        [Required(ErrorMessage = "Old status is required")]
        public DeliveryStatus OldStatus { get; init; }

        [Required(ErrorMessage = "New status is required")]
        public DeliveryStatus NewStatus { get; init; }
    }
}
