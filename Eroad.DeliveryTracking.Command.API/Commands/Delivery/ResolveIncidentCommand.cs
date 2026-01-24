using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record ResolveIncidentCommand : BaseCommand
    {
        [Required(ErrorMessage = "Incident ID is required")]
        public Guid IncidentId { get; init; }
    }
}
