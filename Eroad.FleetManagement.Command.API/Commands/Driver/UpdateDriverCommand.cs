using Eroad.CQRS.Core.Commands;
using System.ComponentModel.DataAnnotations;

namespace Eroad.FleetManagement.Command.API.Commands.Driver
{
    public record UpdateDriverCommand : BaseCommand
    {
        [Required(ErrorMessage = "Driver license is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Driver license must be between 1 and 50 characters")]
        public string DriverLicense { get; init; }
    }
}