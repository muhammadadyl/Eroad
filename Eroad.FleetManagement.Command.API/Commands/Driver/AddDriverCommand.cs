using Eroad.CQRS.Core.Commands;
using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Command.API.Commands.Driver
{
    public record AddDriverCommand : BaseCommand
    {
        public string Name { get; init; }
        public string DriverLicense { get; init; }
        public DriverStatus DriverStatus { get; set; }
    }
}