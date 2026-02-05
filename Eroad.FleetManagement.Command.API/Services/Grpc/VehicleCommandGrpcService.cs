using Eroad.CQRS.Core.Infrastructure;
using Eroad.FleetManagement.Command.API.Commands.Vehicle;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using Eroad.FleetManagement.Contracts;
using Grpc.Core;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Services.Grpc
{
    public class VehicleCommandGrpcService : VehicleCommand.VehicleCommandBase
    {
        private readonly IMediator _mediator;
        private readonly IEventSourcingHandler<VehicleAggregate> _eventSourcingHandler;
        private readonly ILogger<VehicleCommandGrpcService> _logger;

        public VehicleCommandGrpcService(
            IMediator mediator,
            IEventSourcingHandler<VehicleAggregate> eventSourcingHandler,
            ILogger<VehicleCommandGrpcService> logger)
        {
            _mediator = mediator;
            _eventSourcingHandler = eventSourcingHandler;
            _logger = logger;
        }

        public override async Task<AddVehicleResponse> AddVehicle(AddVehicleRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                var command = new AddVehicleCommand
                {
                    Id = vehicleId,
                    Registration = request.Registration,
                    VehicleType = request.VehicleType
                };

                await _mediator.Send(command, context.CancellationToken);

                return new AddVehicleResponse
                {
                    Message = "Vehicle added successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when adding vehicle");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicle");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while adding the vehicle"));
            }
        }

        public override async Task<UpdateVehicleResponse> UpdateVehicle(UpdateVehicleRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                var command = new UpdateVehicleCommand
                {
                    Id = vehicleId,
                    Registration = request.Registration,
                    VehicleType = request.VehicleType
                };

                await _mediator.Send(command, context.CancellationToken);

                return new UpdateVehicleResponse
                {
                    Message = "Vehicle updated successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating vehicle");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the vehicle"));
            }
        }

        public override async Task<ChangeVehicleStatusResponse> ChangeVehicleStatus(ChangeVehicleStatusRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                if (!System.Enum.TryParse<FleetManagement.Common.VehicleStatus>(request.Status, true, out var newStatus))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle status"));
                }

                // Get the current vehicle aggregate to retrieve its current status
                var aggregate = await _eventSourcingHandler.GetByIdAsync(vehicleId);
                if (aggregate == null)
                {
                    _logger.LogInformation("Vehicle with ID {VehicleId} not found", vehicleId);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Vehicle with ID {vehicleId} not found"));
                }

                // Check if the new status is the same as the current status
                if (aggregate.Status == newStatus)
                {
                    _logger.LogInformation("Vehicle {VehicleId} is already in status {Status}. No change needed.", vehicleId, newStatus);
                    return new ChangeVehicleStatusResponse
                    {
                        Message = $"Vehicle is already in {newStatus} status"
                    };
                }

                var command = new ChangeVehicleStatusCommand
                {
                    Id = vehicleId,
                    OldStatus = aggregate.Status,
                    NewStatus = newStatus,
                    Reason = string.Empty
                };

                await _mediator.Send(command, context.CancellationToken);

                _logger.LogInformation("Vehicle {VehicleId} status changed from {OldStatus} to {NewStatus}", vehicleId, aggregate.Status, newStatus);

                return new ChangeVehicleStatusResponse
                {
                    Message = "Vehicle status changed successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation when changing vehicle status for vehicle {VehicleId}: {Message}", request.Id, ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing vehicle status for vehicle {VehicleId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while changing vehicle status"));
            }
        }
    }
}
