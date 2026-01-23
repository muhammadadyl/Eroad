using Eroad.CQRS.Core.Commands;
using Eroad.FleetManagement.Common;
using System.ComponentModel.DataAnnotations;

namespace Eroad.FleetManagement.Command.API.Commands.Driver
{
    public record AddDriverCommand : BaseCommand
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
        public string Name { get; init; }

        [Required(ErrorMessage = "Driver license is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Driver license must be between 1 and 50 characters")]
        public string DriverLicense { get; init; }

        public DriverStatus DriverStatus { get; set; }
    }
}