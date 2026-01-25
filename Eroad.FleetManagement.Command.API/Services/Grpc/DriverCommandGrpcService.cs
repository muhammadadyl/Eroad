using Eroad.FleetManagement.Command.API.Commands;
using Eroad.FleetManagement.Command.API.Commands.Driver;
using Eroad.FleetManagement.Command.API.Commands.Vehicle;
using Eroad.FleetManagement.Contracts;
using Grpc.Core;

namespace Eroad.FleetManagement.Command.API.Services.Grpc
{
    public class DriverCommandGrpcService : DriverCommand.DriverCommandBase
    {
        private readonly IDriverCommandHandler _commandHandler;
        private readonly IVehicleCommandHandler _vehicleCommandHandler;
        private readonly ILogger<DriverCommandGrpcService> _logger;

        public DriverCommandGrpcService(IDriverCommandHandler commandHandler, IVehicleCommandHandler vehicleCommandHandler, ILogger<DriverCommandGrpcService> logger)
        {
            _commandHandler = commandHandler;
            _vehicleCommandHandler = vehicleCommandHandler;
            _logger = logger;
        }

        public override async Task<AddDriverResponse> AddDriver(AddDriverRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var command = new AddDriverCommand
                {
                    Id = driverId,
                    Name = request.Name,
                    DriverLicense = request.DriverLicense,
                    DriverStatus = FleetManagement.Common.DriverStatus.Available
                };

                await _commandHandler.HandleAsync(command);

                return new AddDriverResponse
                {
                    Message = "Driver added successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when adding driver");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding driver");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while adding the driver"));
            }
        }

        public override async Task<UpdateDriverResponse> UpdateDriver(UpdateDriverRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var command = new UpdateDriverCommand
                {
                    Id = driverId,
                    DriverLicense = request.DriverLicense
                };

                await _commandHandler.HandleAsync(command);

                return new UpdateDriverResponse
                {
                    Message = "Driver updated successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating driver");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the driver"));
            }
        }

        public override async Task<ChangeDriverStatusResponse> ChangeDriverStatus(ChangeDriverStatusRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                if (!System.Enum.TryParse<FleetManagement.Common.DriverStatus>(request.Status, true, out var newStatus))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver status"));
                }

                var command = new ChangeDriverStatusCommand
                {
                    Id = driverId,
                    OldStatus = FleetManagement.Common.DriverStatus.Available,
                    NewStatus = newStatus
                };

                await _commandHandler.HandleAsync(command);

                return new ChangeDriverStatusResponse
                {
                    Message = "Driver status changed successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when changing driver status");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing driver status");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while changing driver status"));
            }
        }

        public override async Task<AssignDriverToVehicleResponse> AssignDriverToVehicle(AssignDriverToVehicleRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                if (!Guid.TryParse(request.VehicleId, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                var command = new AssignDriverToVehicleCommand
                {
                    VehicleId = vehicleId,
                    DriverId = driverId
                };

                await _vehicleCommandHandler.HandleAsync(command);

                return new AssignDriverToVehicleResponse
                {
                    Message = "Driver assigned to vehicle successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when assigning driver to vehicle");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver to vehicle");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while assigning driver to vehicle"));
            }
        }
    }
}
