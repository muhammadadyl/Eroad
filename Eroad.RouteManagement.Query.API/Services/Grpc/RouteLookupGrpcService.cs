using Eroad.RouteManagement.Common;
using Eroad.RouteManagement.Contracts;
using Eroad.RouteManagement.Query.API.Queries;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace Eroad.RouteManagement.Query.API.Services.Grpc
{
    public class RouteLookupGrpcService : RouteLookup.RouteLookupBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RouteLookupGrpcService> _logger;

        public RouteLookupGrpcService(
            IMediator mediator,
            ILogger<RouteLookupGrpcService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task<RouteResponse> GetRouteById(GetRouteByIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                var routes = await _mediator.Send(new FindRouteByIdQuery { Id = routeId }, context.CancellationToken);
                var route = routes.FirstOrDefault();
                if (route == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Route with ID {request.Id} not found"));
                }

                return new RouteResponse
                {
                    Message = "Successfully returned route",
                    Route = MapToProto(route)
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving route by ID: {RouteId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the route"));
            }
        }

        public override async Task<RouteLookupResponse> GetAllRoutes(GetAllRoutesRequest request, ServerCallContext context)
        {
            try
            {
                var routes = await _mediator.Send(new FindAllRoutesQuery(), context.CancellationToken);
                
                var response = new RouteLookupResponse
                {
                    Message = $"Successfully returned {routes.Count} route(s)",
                    Routes = { routes.Select(MapToProto) }
                };
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all routes");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving routes"));
            }
        }

        public override async Task<RouteLookupResponse> GetRoutesByStatus(GetRoutesByStatusRequest request, ServerCallContext context)
        {
            try
            {
                var routes = await _mediator.Send(new FindRoutesByStatusQuery { Status = request.Status }, context.CancellationToken);
                
                var response = new RouteLookupResponse
                {
                    Message = $"Successfully returned {routes.Count} route(s) with status '{request.Status}'",
                    Routes = { routes.Select(MapToProto) }
                };
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes by status: {Status}", request.Status);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving routes"));
            }
        }

        public override async Task<CheckpointLookupResponse> GetCheckpointsByRoute(GetCheckpointsByRouteRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.RouteId, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                var checkpoints = await _mediator.Send(new FindCheckpointsByRouteIdQuery { RouteId = routeId }, context.CancellationToken);
                
                var response = new CheckpointLookupResponse
                {
                    Message = $"Successfully returned {checkpoints.Count} checkpoint(s)",
                    Checkpoints = { checkpoints.Select(MapCheckpointToProto) }
                };
                
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving checkpoints by route: {RouteId}", request.RouteId);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving checkpoints"));
            }
        }

        private static RouteEntity MapToProto(Domain.Entities.RouteEntity route)
        {
            var entity = new RouteEntity
            {
                Id = route.Id.ToString(),
                Origin = route.Origin,
                Destination = route.Destination,
                Status = route.Status,
                CreatedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(route.CreatedDate, DateTimeKind.Utc)),
                ScheduledStartTime = Timestamp.FromDateTime(DateTime.SpecifyKind(route.ScheduledStartTime, DateTimeKind.Utc)),
                ScheduledEndTime = Timestamp.FromDateTime(DateTime.SpecifyKind(route.ScheduledEndTime, DateTimeKind.Utc))
            };

            entity.Checkpoints.AddRange(route.Checkpoints.Select(i => new CheckpointEntity
            {
                RouteId = i.RouteId.ToString(),
                Location = i.Location,
                ExpectedTime = Timestamp.FromDateTime(DateTime.SpecifyKind(i.ExpectedTime, DateTimeKind.Utc)),
                Sequence = i.Sequence
            }));

            if (route.UpdatedDate.HasValue)
            {
                entity.UpdatedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(route.UpdatedDate.Value, DateTimeKind.Utc));
            }

            return entity;
        }

        private static CheckpointEntity MapCheckpointToProto(Domain.Entities.CheckpointEntity checkpoint)
        {
            return new CheckpointEntity
            {
                RouteId = checkpoint.RouteId.ToString(),
                Sequence = checkpoint.Sequence,
                Location = checkpoint.Location,
                ExpectedTime = Timestamp.FromDateTime(DateTime.SpecifyKind(checkpoint.ExpectedTime, DateTimeKind.Utc))
            };
        }
    }
}
