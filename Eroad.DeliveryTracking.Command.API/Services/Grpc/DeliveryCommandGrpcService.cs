using Eroad.CQRS.Core.Infrastructure;
using Eroad.DeliveryTracking.Command.API.Commands.Delivery;
using Eroad.DeliveryTracking.Command.Domain.Aggregates;
using Eroad.DeliveryTracking.Contracts;
using Grpc.Core;
using MediatR;

namespace Eroad.DeliveryTracking.Command.API.Services.Grpc
{
    public class DeliveryCommandGrpcService : DeliveryCommand.DeliveryCommandBase
    {
        private readonly IMediator _mediator;
        private readonly IEventSourcingHandler<DeliveryAggregate> _eventSourcingHandler;
        private readonly ILogger<DeliveryCommandGrpcService> _logger;

        public DeliveryCommandGrpcService(
            IMediator mediator, 
            IEventSourcingHandler<DeliveryAggregate> eventSourcingHandler,
            ILogger<DeliveryCommandGrpcService> logger)
        {
            _mediator = mediator;
            _eventSourcingHandler = eventSourcingHandler;
            _logger = logger;
        }

        public override async Task<CreateDeliveryResponse> CreateDelivery(CreateDeliveryRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                if (!Guid.TryParse(request.RouteId, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                Guid? driverId = null;
                if (!string.IsNullOrEmpty(request.DriverId) && Guid.TryParse(request.DriverId, out var parsedDriverId))
                {
                    driverId = parsedDriverId;
                }

                Guid? vehicleId = null;
                if (!string.IsNullOrEmpty(request.VehicleId) && Guid.TryParse(request.VehicleId, out var parsedVehicleId))
                {
                    vehicleId = parsedVehicleId;
                }

                var command = new CreateDeliveryCommand
                {
                    Id = deliveryId,
                    RouteId = routeId,
                    DriverId = driverId,
                    VehicleId = vehicleId
                };

                await _mediator.Send(command, context.CancellationToken);

                return new CreateDeliveryResponse
                {
                    Message = "Delivery created successfully",
                    Id = deliveryId.ToString()
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating delivery");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delivery");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the delivery"));
            }
        }

        public override async Task<UpdateDeliveryStatusResponse> UpdateDeliveryStatus(UpdateDeliveryStatusRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                if (!System.Enum.TryParse<DeliveryTracking.Common.DeliveryStatus>(request.Status, true, out var newStatus))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery status"));
                }

                // Get the current delivery aggregate to retrieve its current status
                var aggregate = await _eventSourcingHandler.GetByIdAsync(deliveryId);
                if (aggregate == null)
                {
                    _logger.LogInformation("Delivery with ID {DeliveryId} not found", deliveryId);
                    throw new RpcException(new Status(StatusCode.NotFound, $"Delivery with ID {deliveryId} not found"));
                }

                // Check if the new status is the same as the current status
                if (aggregate.Status == newStatus)
                {
                    _logger.LogInformation("Delivery {DeliveryId} is already in status {Status}. No change needed.", deliveryId, newStatus);
                    return new UpdateDeliveryStatusResponse
                    {
                        Message = $"Delivery is already in {newStatus} status"
                    };
                }

                var command = new UpdateDeliveryStatusCommand
                {
                    Id = deliveryId,
                    OldStatus = aggregate.Status,
                    NewStatus = newStatus
                };

                await _mediator.Send(command, context.CancellationToken);

                _logger.LogInformation("Delivery {DeliveryId} status changed from {OldStatus} to {NewStatus}", deliveryId, aggregate.Status, newStatus);

                return new UpdateDeliveryStatusResponse
                {
                    Message = "Delivery status updated successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation when updating delivery status for delivery {DeliveryId}: {Message}", request.Id, ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status for delivery {DeliveryId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating delivery status"));
            }
        }

        public override async Task<UpdateCurrentCheckpointResponse> UpdateCurrentCheckpoint(UpdateCurrentCheckpointRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                if (!Guid.TryParse(request.RouteId, out var routeId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
                }

                var command = new UpdateCurrentCheckpointCommand
                {
                    Id = deliveryId,
                    RouteId = routeId,
                    Sequence = request.Sequence,
                    Location = request.Location
                };

                await _mediator.Send(command, context.CancellationToken);

                return new UpdateCurrentCheckpointResponse
                {
                    Message = "Checkpoint reached successfully"
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

        public override async Task<ReportIncidentResponse> ReportIncident(ReportIncidentRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                var command = new ReportIncidentCommand
                {
                    Id = deliveryId,
                    Incident = new DeliveryTracking.Common.Incident
                    {
                        Id = Guid.NewGuid(),
                        Type = request.Type,
                        Description = request.Description,
                        ReportedTimestamp = DateTime.UtcNow,
                        Resolved = false
                    }
                };

                await _mediator.Send(command, context.CancellationToken);

                return new ReportIncidentResponse
                {
                    Message = "Incident reported successfully",
                    Id = command.Incident.Id.ToString()
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when reporting incident");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting incident");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while reporting the incident"));
            }
        }

        public override async Task<ResolveIncidentResponse> ResolveIncident(ResolveIncidentRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                if (!Guid.TryParse(request.IncidentId, out var incidentId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid incident ID format"));
                }

                var command = new ResolveIncidentCommand
                {
                    Id = deliveryId,
                    IncidentId = incidentId
                };

                await _mediator.Send(command, context.CancellationToken);

                return new ResolveIncidentResponse
                {
                    Message = "Incident resolved successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when resolving incident");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving incident");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while resolving the incident"));
            }
        }

        public override async Task<CaptureProofOfDeliveryResponse> CaptureProofOfDelivery(CaptureProofOfDeliveryRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                var command = new CaptureProofOfDeliveryCommand
                {
                    Id = deliveryId,
                    SignatureUrl = request.SignatureUrl,
                    ReceiverName = request.ReceiverName
                };

                await _mediator.Send(command, context.CancellationToken);

                return new CaptureProofOfDeliveryResponse
                {
                    Message = "Proof of delivery captured successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when capturing proof of delivery");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing proof of delivery");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while capturing proof of delivery"));
            }
        }

        public override async Task<AssignDriverResponse> AssignDriver(AssignDriverRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                if (!Guid.TryParse(request.DriverId, out var driverId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
                }

                var command = new AssignDriverCommand
                {
                    Id = deliveryId,
                    DriverId = driverId
                };

                await _mediator.Send(command, context.CancellationToken);

                _logger.LogInformation("Driver {DriverId} assigned to delivery {DeliveryId}", driverId, deliveryId);

                return new AssignDriverResponse
                {
                    Message = "Driver assigned successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when assigning driver");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver to delivery {DeliveryId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while assigning driver"));
            }
        }

        public override async Task<AssignVehicleResponse> AssignVehicle(AssignVehicleRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var deliveryId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
                }

                if (!Guid.TryParse(request.VehicleId, out var vehicleId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
                }

                var command = new AssignVehicleCommand
                {
                    Id = deliveryId,
                    VehicleId = vehicleId
                };

                await _mediator.Send(command, context.CancellationToken);

                _logger.LogInformation("Vehicle {VehicleId} assigned to delivery {DeliveryId}", vehicleId, deliveryId);

                return new AssignVehicleResponse
                {
                    Message = "Vehicle assigned successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when assigning vehicle");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning vehicle to delivery {DeliveryId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while assigning vehicle"));
            }
        }
    }
}
