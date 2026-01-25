using Eroad.RouteManagement.Command.API.Commands;
using Eroad.RouteManagement.Command.API.Commands.Route;
using Eroad.RouteManagement.Contracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Eroad.RouteManagement.Command.API.Services.Grpc
{
    public class RouteCommandGrpcService : RouteCommand.RouteCommandBase
    {
        private readonly IRouteCommandHandler _commandHandler;
        private readonly ILogger<RouteCommandGrpcService> _logger;

        public RouteCommandGrpcService(IRouteCommandHandler commandHandler, ILogger<RouteCommandGrpcService> logger)
        {
            _commandHandler = commandHandler;
            _logger = logger;
        }

        public override async Task<CreateRouteResponse> CreateRoute(CreateRouteRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                var command = new CreateRouteCommand
                {
                    Id = routeId,
                    Origin = request.Origin,
                    Destination = request.Destination,
                    AssignedDriverId = Guid.Empty,
                    AssignedVehicleId = Guid.Empty
                };

                await _commandHandler.HandleAsync(command);

                return new CreateRouteResponse
                {
                    Message = "Route created successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating route");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the route"));
            }
        }

        public override async Task<UpdateRouteResponse> UpdateRoute(UpdateRouteRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                var command = new UpdateRouteCommand
                {
                    Id = routeId,
                    Origin = request.Origin,
                    Destination = request.Destination
                };

                await _commandHandler.HandleAsync(command);

                return new UpdateRouteResponse
                {
                    Message = "Route updated successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating route");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the route"));
            }
        }

        public override async Task<ChangeRouteStatusResponse> ChangeRouteStatus(ChangeRouteStatusRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                if (!System.Enum.TryParse<RouteManagement.Common.RouteStatus>(request.Status, true, out var newStatus))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route status"));
                }

                var command = new ChangeRouteStatusCommand
                {
                    Id = routeId,
                    OldStatus = RouteManagement.Common.RouteStatus.Planned,
                    NewStatus = newStatus
                };

                await _commandHandler.HandleAsync(command);

                return new ChangeRouteStatusResponse
                {
                    Message = "Route status changed successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when changing route status");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing route status");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while changing route status"));
            }
        }

        public override async Task<AddCheckpointResponse> AddCheckpoint(AddCheckpointRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                var checkpoint = new RouteManagement.Common.Checkpoint
                {
                    Sequence = request.Sequence,
                    Location = request.Location,
                    ExpectedTime = request.ExpectedTime.ToDateTime()
                };

                var command = new AddCheckpointCommand
                {
                    Id = routeId,
                    Checkpoint = checkpoint
                };

                await _commandHandler.HandleAsync(command);

                return new AddCheckpointResponse
                {
                    Message = "Checkpoint added successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when adding checkpoint");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding checkpoint");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while adding checkpoint"));
            }
        }

        public override async Task<UpdateCheckpointResponse> UpdateCheckpoint(UpdateCheckpointRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                DateTime? actualTime = null;
                if (request.ActualTime != null)
                {
                    actualTime = request.ActualTime.ToDateTime();
                }

                var command = new UpdateCheckpointCommand
                {
                    Id = routeId,
                    Sequence = request.Sequence,
                    ActualTime = actualTime
                };

                await _commandHandler.HandleAsync(command);

                return new UpdateCheckpointResponse
                {
                    Message = "Checkpoint updated successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating checkpoint");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating checkpoint");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating checkpoint"));
            }
        }

        public override async Task<AssignDriverToRouteResponse> AssignDriverToRoute(AssignDriverToRouteRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                if (!Guid.TryParse(request.DriverId, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var command = new AssignDriverToRouteCommand
                {
                    Id = routeId,
                    DriverId = driverId
                };

                await _commandHandler.HandleAsync(command);

                return new AssignDriverToRouteResponse
                {
                    Message = "Driver assigned to route successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when assigning driver to route");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver to route");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while assigning driver to route"));
            }
        }

        public override async Task<AssignVehicleToRouteResponse> AssignVehicleToRoute(AssignVehicleToRouteRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                if (!Guid.TryParse(request.VehicleId, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                var command = new AssignVehicleToRouteCommand
                {
                    Id = routeId,
                    VehicleId = vehicleId
                };

                await _commandHandler.HandleAsync(command);

                return new AssignVehicleToRouteResponse
                {
                    Message = "Vehicle assigned to route successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when assigning vehicle to route");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning vehicle to route");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while assigning vehicle to route"));
            }
        }
    }
}
