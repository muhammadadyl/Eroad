using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Commands.Vehicle.Handlers
{
    public class AssignDriverToVehicleCommandHandler : IRequestHandler<AssignDriverToVehicleCommand>
    {
        private readonly IEventSourcingHandler<VehicleAggregate> _eventSourcingHandler;
        private readonly IEventSourcingHandler<DriverAggregate> _driverEventSourcingHandler;

        public AssignDriverToVehicleCommandHandler(
            IEventSourcingHandler<VehicleAggregate> eventSourcingHandler,
            IEventSourcingHandler<DriverAggregate> driverEventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
            _driverEventSourcingHandler = driverEventSourcingHandler;
        }

        public async Task Handle(AssignDriverToVehicleCommand request, CancellationToken cancellationToken)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(request.VehicleId);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Vehicle aggregate with ID {request.VehicleId} not found.");
            }

            if (request.DriverId != Guid.Empty)
            {
                var driverAggregate = await _driverEventSourcingHandler.GetByIdAsync(request.DriverId);
                if (driverAggregate == null)
                {
                    throw new AggregateNotFoundException($"Driver aggregate with ID {request.DriverId} not found.");
                }

                if (driverAggregate.Status != Common.DriverStatus.Available)
                {
                    throw new InvalidOperationException($"Driver with ID {request.DriverId} is not active and cannot be assigned to a vehicle.");
                }

                if (aggregate.AssignedDriverId != Guid.Empty)
                {
                    var oldAssignedDriver = await _driverEventSourcingHandler.GetByIdAsync(aggregate.AssignedDriverId);
                    if (oldAssignedDriver != null && oldAssignedDriver.Status == Common.DriverStatus.Assigned)
                    {
                        oldAssignedDriver.ChangeDriverStatus(oldAssignedDriver.Status, Common.DriverStatus.Available);
                        await _driverEventSourcingHandler.SaveAsync(oldAssignedDriver);
                    }
                }

                driverAggregate.ChangeDriverStatus(driverAggregate.Status, Common.DriverStatus.Assigned);
                await _driverEventSourcingHandler.SaveAsync(driverAggregate);

                aggregate.AssignDriver(request.DriverId);
                await _eventSourcingHandler.SaveAsync(aggregate);
            }
        }
    }
}
