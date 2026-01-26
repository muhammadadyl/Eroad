using Eroad.CQRS.Core.Handlers;
using Eroad.FleetManagement.Command.API.Commands.Driver;
using Eroad.FleetManagement.Command.Domain.Aggregates;
using Eroad.FleetManagement.Contracts;
using Grpc.Core;
using MediatR;

namespace Eroad.FleetManagement.Command.API.Services.Grpc
{
    public class DriverCommandGrpcService : DriverCommand.DriverCommandBase
    {
        private readonly IMediator _mediator;
        private readonly IEventSourcingHandler<DriverAggregate> _eventSourcingHandler;
        private readonly ILogger<DriverCommandGrpcService> _logger;

        public DriverCommandGrpcService(
            IMediator mediator,
            IEventSourcingHandler<DriverAggregate> eventSourcingHandler,
            ILogger<DriverCommandGrpcService> logger)
        {
            _mediator = mediator;
            _eventSourcingHandler = eventSourcingHandler;
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

                await _mediator.Send(command, context.CancellationToken);

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

                await _mediator.Send(command, context.CancellationToken);

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

                // Get the current driver aggregate to retrieve its current status
                var aggregate = await _eventSourcingHandler.GetByIdAsync(driverId);
                if (aggregate == null)
                {
                    _logger.LogInformation("Driver with ID {DriverId} not found", driverId);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Driver with ID {driverId} not found"));
                }

                // Check if the new status is the same as the current status
                if (aggregate.Status == newStatus)
                {
                    _logger.LogInformation("Driver {DriverId} is already in status {Status}. No change needed.", driverId, newStatus);
                    return new ChangeDriverStatusResponse
                    {
                        Message = $"Driver is already in {newStatus} status"
                    };
                }

                var command = new ChangeDriverStatusCommand
                {
                    Id = driverId,
                    OldStatus = aggregate.Status,
                    NewStatus = newStatus
                };

                await _mediator.Send(command, context.CancellationToken);

                _logger.LogInformation("Driver {DriverId} status changed from {OldStatus} to {NewStatus}", driverId, aggregate.Status, newStatus);

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
                _logger.LogWarning("Invalid operation when changing driver status for driver {DriverId}: {Message}", request.Id, ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing driver status for driver {DriverId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while changing driver status"));
            }
        }
    }
}
