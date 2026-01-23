using Eroad.CQRS.Core.Commands;
using Eroad.DeliveryTracking.Common;
using System.ComponentModel.DataAnnotations;

namespace Eroad.DeliveryTracking.Command.API.Commands.Delivery
{
    public record ReportIncidentCommand : BaseCommand
    {
        [Required(ErrorMessage = "Incident is required")]
        public Incident Incident { get; init; }
    }
}
