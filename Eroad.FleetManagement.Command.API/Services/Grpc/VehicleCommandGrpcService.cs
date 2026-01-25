using Eroad.FleetManagement.Command.API.Commands;
using Eroad.FleetManagement.Command.API.Commands.Vehicle;
using Eroad.FleetManagement.Contracts;
using Grpc.Core;

namespace Eroad.FleetManagement.Command.API.Services.Grpc
{
    public class VehicleCommandGrpcService : VehicleCommand.VehicleCommandBase
    {
        private readonly IVehicleCommandHandler _commandHandler;
        private readonly ILogger<VehicleCommandGrpcService> _logger;

        public VehicleCommandGrpcService(IVehicleCommandHandler commandHandler, ILogger<VehicleCommandGrpcService> logger)
        {
            _commandHandler = commandHandler;
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

                await _commandHandler.HandleAsync(command);

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

                await _commandHandler.HandleAsync(command);

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

                var command = new ChangeVehicleStatusCommand
                {
                    Id = vehicleId,
                    OldStatus = FleetManagement.Common.VehicleStatus.Available,
                    NewStatus = newStatus,
                    Reason = string.Empty
                };

                await _commandHandler.HandleAsync(command);

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
                _logger.LogWarning(ex, "Invalid operation when changing vehicle status");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing vehicle status");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while changing vehicle status"));
            }
        }
    }
}
