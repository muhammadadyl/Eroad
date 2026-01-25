using Eroad.RouteManagement.Contracts;
using Eroad.RouteManagement.Query.Domain.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Eroad.RouteManagement.Query.API.Services.Grpc
{
    public class RouteLookupGrpcService : RouteLookup.RouteLookupBase
    {
        private readonly IRouteRepository _routeRepository;
        private readonly ICheckpointRepository _checkpointRepository;
        private readonly ILogger<RouteLookupGrpcService> _logger;

        public RouteLookupGrpcService(
            IRouteRepository routeRepository,
            ICheckpointRepository checkpointRepository,
            ILogger<RouteLookupGrpcService> logger)
        {
            _routeRepository = routeRepository;
            _checkpointRepository = checkpointRepository;
            _logger = logger;
        }

        public override async Task<RouteLookupResponse> GetRouteById(GetRouteByIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                var route = await _routeRepository.GetByIdAsync(routeId);
                if (route == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Route with ID {request.Id} not found"));
                }

                return new RouteLookupResponse
                {
                    Message = "Successfully returned route",
                    Routes = { MapToProto(route) }
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
                var routes = await _routeRepository.GetAllAsync();
                
                var response = new RouteLookupResponse
                {
                    Message = $"Successfully returned {routes.Count} route(s)"
                };
                response.Routes.AddRange(routes.Select(MapToProto));
                
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
                var routes = await _routeRepository.GetByStatusAsync(request.Status);
                
                var response = new RouteLookupResponse
                {
                    Message = $"Successfully returned {routes.Count} route(s) with status '{request.Status}'"
                };
                response.Routes.AddRange(routes.Select(MapToProto));
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes by status: {Status}", request.Status);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving routes"));
            }
        }

        public override async Task<RouteLookupResponse> GetRoutesByDriver(GetRoutesByDriverRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.DriverId, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var routes = await _routeRepository.GetByDriverIdAsync(driverId);
                
                var response = new RouteLookupResponse
                {
                    Message = $"Successfully returned {routes.Count} route(s)"
                };
                response.Routes.AddRange(routes.Select(MapToProto));
                
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes by driver: {DriverId}", request.DriverId);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving routes"));
            }
        }

        public override async Task<RouteLookupResponse> GetRoutesByVehicle(GetRoutesByVehicleRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.VehicleId, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                var routes = await _routeRepository.GetByVehicleIdAsync(vehicleId);
                
                var response = new RouteLookupResponse
                {
                    Message = $"Successfully returned {routes.Count} route(s)"
                };
                response.Routes.AddRange(routes.Select(MapToProto));
                
                return response;
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes by vehicle: {VehicleId}", request.VehicleId);
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

                var checkpoints = await _checkpointRepository.GetByRouteIdAsync(routeId);
                
                var response = new CheckpointLookupResponse
                {
                    Message = $"Successfully returned {checkpoints.Count} checkpoint(s)"
                };
                response.Checkpoints.AddRange(checkpoints.Select(MapCheckpointToProto));
                
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
                CreatedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(route.CreatedDate, DateTimeKind.Utc))
            };

            if (route.AssignedDriverId.HasValue)
            {
                entity.AssignedDriverId = route.AssignedDriverId.Value.ToString();
            }

            if (route.AssignedVehicleId.HasValue)
            {
                entity.AssignedVehicleId = route.AssignedVehicleId.Value.ToString();
            }

            if (route.CompletedDate.HasValue)
            {
                entity.CompletedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(route.CompletedDate.Value, DateTimeKind.Utc));
            }

            return entity;
        }

        private static CheckpointEntity MapCheckpointToProto(Domain.Entities.CheckpointEntity checkpoint)
        {
            var entity = new CheckpointEntity
            {
                RouteId = checkpoint.RouteId.ToString(),
                Sequence = checkpoint.Sequence,
                Location = checkpoint.Location,
                ExpectedTime = Timestamp.FromDateTime(DateTime.SpecifyKind(checkpoint.ExpectedTime, DateTimeKind.Utc))
            };

            if (checkpoint.ActualTime.HasValue)
            {
                entity.ActualTime = Timestamp.FromDateTime(DateTime.SpecifyKind(checkpoint.ActualTime.Value, DateTimeKind.Utc));
            }

            return entity;
        }
    }
}
