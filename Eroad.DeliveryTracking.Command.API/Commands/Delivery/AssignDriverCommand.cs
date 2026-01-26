using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record AssignDriverCommand : BaseCommand
    {
        [Required(ErrorMessage = "Delivery ID is required")]
        public new Guid Id { get; init; }

        [Required(ErrorMessage = "Driver ID is required")]
        public Guid DriverId { get; init; }
    }
}
