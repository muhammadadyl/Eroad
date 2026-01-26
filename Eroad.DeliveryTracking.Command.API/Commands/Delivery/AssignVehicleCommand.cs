using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record AssignVehicleCommand : BaseCommand
    {
        [Required(ErrorMessage = "Delivery ID is required")]
        public new Guid Id { get; init; }

        [Required(ErrorMessage = "Vehicle ID is required")]
        public Guid VehicleId { get; init; }
    }
}
