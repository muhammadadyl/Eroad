using Eroad.CQRS.Core.Exceptions;
using Eroad.CQRS.Core.Handlers;
using Eroad.RouteManagement.Command.Domain.Aggregates;

namespace Eroad.RouteManagement.Command.API.Commands.Route
{
    public class RouteCommandHandler : IRouteCommandHandler
    {
        private readonly IEventSourcingHandler<RouteAggregate> _eventSourcingHandler;

        public RouteCommandHandler(IEventSourcingHandler<RouteAggregate> eventSourcingHandler)
        {
            _eventSourcingHandler = eventSourcingHandler;
        }

        public async Task HandleAsync(CreateRouteCommand command)
        {
            var aggregate = new RouteAggregate(command.Id, command.Origin, command.Destination, 
                command.AssignedDriverId, command.AssignedVehicleId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(UpdateRouteCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.UpdateRouteInfo(command.Origin, command.Destination);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(ChangeRouteStatusCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.ChangeRouteStatus(command.OldStatus, command.NewStatus);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(AddCheckpointCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.AddCheckpoint(command.Checkpoint);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(UpdateCheckpointCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.UpdateCheckpoint(command.Sequence, command.ActualTime);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(AssignDriverToRouteCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.AssignDriver(command.DriverId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }

        public async Task HandleAsync(AssignVehicleToRouteCommand command)
        {
            var aggregate = await _eventSourcingHandler.GetByIdAsync(command.Id);
            if (aggregate == null)
            {
                throw new AggregateNotFoundException($"Route aggregate with ID {command.Id} not found.");
            }

            aggregate.AssignVehicle(command.VehicleId);
            await _eventSourcingHandler.SaveAsync(aggregate);
        }
    }
}
