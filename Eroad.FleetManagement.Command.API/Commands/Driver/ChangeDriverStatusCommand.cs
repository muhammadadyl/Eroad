using Eroad.CQRS.Core.Commands;
using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Command.API.Commands.Driver
{
    public record ChangeDriverStatusCommand : BaseCommand
    {
        public DriverStatus OldStatus { get; set; }
        public DriverStatus NewStatus { get; set; }
    }
}