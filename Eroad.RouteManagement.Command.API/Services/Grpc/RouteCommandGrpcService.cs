using Eroad.CQRS.Core.Handlers;
using Eroad.RouteManagement.Command.API.Commands.Route;
using Eroad.RouteManagement.Command.Domain.Aggregates;
using Eroad.RouteManagement.Contracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace Eroad.RouteManagement.Command.API.Services.Grpc
{
    public class RouteCommandGrpcService : RouteCommand.RouteCommandBase
    {
        private readonly IMediator _mediator;
        private readonly IEventSourcingHandler<RouteAggregate> _eventSourcingHandler;
        private readonly ILogger<RouteCommandGrpcService> _logger;

        public RouteCommandGrpcService(
            IMediator mediator, 
            IEventSourcingHandler<RouteAggregate> eventSourcingHandler,
            ILogger<RouteCommandGrpcService> logger)
        {
            _mediator = mediator;
            _eventSourcingHandler = eventSourcingHandler;
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
                    Destination = request.Destination
                };

                await _mediator.Send(command, context.CancellationToken);

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

                await _mediator.Send(command, context.CancellationToken);

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

                // Get the current route aggregate to retrieve its current status
                var aggregate = await _eventSourcingHandler.GetByIdAsync(routeId);
                if (aggregate == null)
                {
                    _logger.LogInformation("Route with ID {RouteId} not found", routeId);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Route with ID {routeId} not found"));
                }

                // Check if the new status is the same as the current status
                if (aggregate.Status == newStatus)
                {
                    _logger.LogInformation("Route {RouteId} is already in status {Status}. No change needed.", routeId, newStatus);
                    return new ChangeRouteStatusResponse
                    {
                        Message = $"Route is already in {newStatus} status"
                    };
                }

                var command = new ChangeRouteStatusCommand
                {
                    Id = routeId,
                    OldStatus = aggregate.Status,
                    NewStatus = newStatus
                };

                await _mediator.Send(command, context.CancellationToken);

                _logger.LogInformation("Route {RouteId} status changed from {OldStatus} to {NewStatus}", routeId, aggregate.Status, newStatus);

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
                _logger.LogWarning("Invalid operation when changing route status for route {RouteId}: {Message}", request.Id, ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing route status for route {RouteId}", request.Id);
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

                await _mediator.Send(command, context.CancellationToken);

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
    }
}
