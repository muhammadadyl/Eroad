
using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.Domain.Aggregates;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle
{
    public class VehicleCommandHandler : IVehicleCommandHandler
    {
        private readonly IEventSourcingHandler<VehicleAggregate> _eventSourcingHandler;
        private readonly IEventSourcingHandler<DriverAggregate> _driverEventSourcingHandler;

        public VehicleCommandHandler(IEventSourcingHandler<VehicleAggregate> eventSourcingHandler, IEventSourcingHandler<DriverAggregate> driverEventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
            _driverEventSourcingHandler = driverEventSourcingHandler;
        }
        public async Task HandleAsync(AddVehicleCommand command)
        {
            var aggregate = new VehicleAggregate(command.Id, command.Registration, command.VehicleType);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(UpdateVehicleCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate), $"Vehicle aggregate with ID {command.Id} not found.");
            }

            aggregate.UpdateVehicleInfo(command.Registration, command.VehicleType);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(ChangeVehicleStatusCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate), $"Vehicle aggregate with ID {command.Id} not found.");
            }

            aggregate.ChangeVehicleStatus(command.OldStatus, command.NewStatus, command.Reason);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(AssignDriverToVehicleCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.VehicleId);
            if (aggregate == null)
            {
                throw new ArgumentNullException(nameof(aggregate), $"Vehicle aggregate with ID {command.VehicleId} not found.");
            }

            if (command.DriverId != Guid.Empty)
            {
                var driverAggregate = await _driverEventSourcingHandler.GetByIdAsync(command.DriverId);
                if (driverAggregate == null)
                {
                    throw new ArgumentNullException(nameof(driverAggregate), $"Driver aggregate with ID {command.DriverId} not found.");
                }

                if (driverAggregate.Status != Common.DriverStatus.Available)
                {
                    throw new InvalidOperationException($"Driver with ID {command.DriverId} is not active and cannot be assigned to a vehicle.");
                }

                driverAggregate.ChangeDriverStatus(driverAggregate.Status, Common.DriverStatus.Assigned);
                await _driverEventSourcingHandler.SaveAsync(driverAggregate);

                aggregate.AssignDriver(command.DriverId);
                await _eventSourcingHandler.SaveAsync(aggregate);
            }
        }
    }
}
