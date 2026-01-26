using Eroad.RouteManagement.Common;
using Eroad.RouteManagement.Query.Domain.Entities;
using Eroad.RouteManagement.Query.Domain.Repositories;

namespace Eroad.RouteManagement.Query.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IRouteRepository _routeRepository;
        private readonly ICheckpointRepository _checkpointRepository;

        public EventHandler(IRouteRepository routeRepository, ICheckpointRepository checkpointRepository)
        {
            _routeRepository = routeRepository;
            _checkpointRepository = checkpointRepository;
        }

        public async Task On(RouteCreatedEvent @event)
        {
            var route = new RouteEntity
            {
                Id = @event.Id,
                Origin = @event.Origin,
                Destination = @event.Destination,
                Status = @event.RouteStatus.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _routeRepository.CreateAsync(route);
        }

        public async Task On(RouteUpdatedEvent @event)
        {
            var route = await _routeRepository.GetByIdAsync(@event.Id);

            if (route == null) return;

            route.Origin = @event.Origin;
            route.Destination = @event.Destination;

            await _routeRepository.UpdateAsync(route);
        }

        public async Task On(RouteStatusChangedEvent @event)
        {
            var route = await _routeRepository.GetByIdAsync(@event.Id);

            if (route == null) return;

            route.Status = @event.NewStatus.ToString();

            if (@event.NewStatus == RouteStatus.Completed)
            {
                route.CompletedDate = DateTime.UtcNow;
            }

            await _routeRepository.UpdateAsync(route);
        }

        public async Task On(CheckpointAddedEvent @event)
        {
            var checkpoint = new CheckpointEntity
            {
                RouteId = @event.Id,
                Sequence = @event.Checkpoint.Sequence,
                Location = @event.Checkpoint.Location,
                ExpectedTime = @event.Checkpoint.ExpectedTime
            };

            await _checkpointRepository.CreateAsync(checkpoint);
        }
    }
}
