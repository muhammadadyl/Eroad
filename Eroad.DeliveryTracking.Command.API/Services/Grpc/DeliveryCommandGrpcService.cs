using Eroad.DeliveryTracking.Command.API.Commands.Delivery;
using Eroad.DeliveryTracking.Contracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Eroad.DeliveryTracking.Command.API.Services.Grpc
{
    public class DeliveryCommandGrpcService : DeliveryCommand.DeliveryCommandBase
    {
        private readonly IDeliveryCommandHandler _commandHandler;
        private readonly ILogger<DeliveryCommandGrpcService> _logger;

        public DeliveryCommandGrpcService(IDeliveryCommandHandler commandHandler, ILogger<DeliveryCommandGrpcService> logger)
        {
            _commandHandler = commandHandler;
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

                var command = new CreateDeliveryCommand
                {
                    Id = deliveryId,
                    RouteId = routeId
                };

                await _commandHandler.HandleAsync(command);

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

                // Note: Proto doesn't have OldStatus, using PickedUp as default
                var command = new UpdateDeliveryStatusCommand
                {
                    Id = deliveryId,
                    OldStatus = DeliveryTracking.Common.DeliveryStatus.PickedUp,
                    NewStatus = newStatus
                };

                await _commandHandler.HandleAsync(command);

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
                _logger.LogWarning(ex, "Invalid operation when updating delivery status");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status");
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

                var command = new UpdateCurrentCheckpointCommand
                {
                    Id = deliveryId,
                    Checkpoint = request.Checkpoint
                };

                await _commandHandler.HandleAsync(command);

                return new UpdateCurrentCheckpointResponse
                {
                    Message = "Current checkpoint updated successfully"
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating current checkpoint");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current checkpoint");
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating current checkpoint"));
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

                await _commandHandler.HandleAsync(command);

                return new ReportIncidentResponse
                {
                    Message = "Incident reported successfully"
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

                await _commandHandler.HandleAsync(command);

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

                await _commandHandler.HandleAsync(command);

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
    }
}
