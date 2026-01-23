using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record CaptureProofOfDeliveryCommand : BaseCommand
    {
        [Required(ErrorMessage = "Signature URL is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Signature URL must be between 1 and 500 characters")]
        public string SignatureUrl { get; init; }

        [Required(ErrorMessage = "Receiver name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Receiver name must be between 1 and 200 characters")]
        public string ReceiverName { get; init; }
    }
}
