using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record ResolveIncidentCommand : BaseCommand
    {
        [Required(ErrorMessage = "Incident type is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Incident type must be between 1 and 100 characters")]
        public string IncidentType { get; init; }

        [Required(ErrorMessage = "Timestamp is required")]
        public DateTime Timestamp { get; init; }
    }
}
