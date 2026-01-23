using Eroad.CQRS.Core.Commands;

namespace Eroad.FleetManagement.Command.API.Commands.Driver
{
    public record UpdateDriverCommand : BaseCommand
    {
        public string DriverLicense { get; init; }
    }
}