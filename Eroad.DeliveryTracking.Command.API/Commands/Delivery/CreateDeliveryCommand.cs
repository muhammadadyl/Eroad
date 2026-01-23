using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record CreateDeliveryCommand : BaseCommand
    {
        [Required(ErrorMessage = "Route ID is required")]
        public Guid RouteId { get; init; }
    }
}
