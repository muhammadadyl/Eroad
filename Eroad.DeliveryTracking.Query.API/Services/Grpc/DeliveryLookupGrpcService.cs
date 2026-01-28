using Eroad.DeliveryTracking.Contracts;
using Eroad.DeliveryTracking.Query.API.Queries;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Services.Grpc;

public class DeliveryLookupGrpcService : DeliveryLookup.DeliveryLookupBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeliveryLookupGrpcService> _logger;

    public DeliveryLookupGrpcService(IMediator mediator, ILogger<DeliveryLookupGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<DeliveryLookupResponse> GetDeliveryById(GetDeliveryByIdRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var deliveryId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
            }

            var deliveries = await _mediator.Send(new FindDeliveryByIdQuery { Id = deliveryId }, context.CancellationToken);
            if (deliveries == null || deliveries.Count == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Delivery {request.Id} not found"));
            }

            return new DeliveryLookupResponse
            {
                Message = "Delivery retrieved successfully",
                Deliveries = { deliveries.Select(MapToProto) }
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery by ID: {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving delivery"));
        }
    }

    public override async Task<DeliveryLookupResponse> GetAllDeliveries(GetAllDeliveriesRequest request, ServerCallContext context)
    {
        try
        {
            var deliveries = await _mediator.Send(new FindAllDeliveriesQuery(), context.CancellationToken);
            return new DeliveryLookupResponse
            {
                Message = "Deliveries retrieved successfully",
                Deliveries = { deliveries.Select(MapToProto) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all deliveries");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving deliveries"));
        }
    }

    public override async Task<DeliveryEventLogsResponse> GetDeliveryEventLogs(GetDeliveryEventLogsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DeliveryId, out var deliveryId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
            }

            var eventLogs = await _mediator.Send(new FindDeliveryEventLogsQuery { DeliveryId = deliveryId }, context.CancellationToken);

            var response = new DeliveryEventLogsResponse
            {
                Message = $"Found {eventLogs.Count} event log(s)"
            };

            foreach (var log in eventLogs)
            {
                response.EventLogs.Add(new DeliveryEventLogEntity
                {
                    Id = log.Id.ToString(),
                    DeliveryId = log.DeliveryId.ToString(),
                    EventCategory = log.EventCategory,
                    EventType = log.EventType,
                    EventData = log.EventData,
                    OccurredAt = Timestamp.FromDateTime(DateTime.SpecifyKind(log.OccurredAt, DateTimeKind.Utc))
                });
            }

            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delivery event logs for delivery {DeliveryId}", request.DeliveryId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving event logs"));
        }
    }

    public override async Task<DeliveryLookupResponse> GetActiveDeliveriesByDriver(GetActiveDeliveriesByDriverRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DriverId, out var driverId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid driver ID format"));
            }

            var deliveries = await _mediator.Send(new FindActiveDeliveriesByDriverQuery { DriverId = driverId }, context.CancellationToken);
            return new DeliveryLookupResponse
            {
                Message = $"Found {deliveries.Count} active delivery(ies) for driver",
                Deliveries = { deliveries.Select(MapToProto) }
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active deliveries for driver {DriverId}", request.DriverId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving deliveries"));
        }
    }

    public override async Task<DeliveryLookupResponse> GetActiveDeliveriesByVehicle(GetActiveDeliveriesByVehicleRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.VehicleId, out var vehicleId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid vehicle ID format"));
            }

            var deliveries = await _mediator.Send(new FindActiveDeliveriesByVehicleQuery { VehicleId = vehicleId }, context.CancellationToken);
            return new DeliveryLookupResponse
            {
                Message = $"Found {deliveries.Count} active delivery(ies) for vehicle",
                Deliveries = { deliveries.Select(MapToProto) }
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active deliveries for vehicle {VehicleId}", request.VehicleId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving deliveries"));
        }
    }

    private static DeliveryEntity MapToProto(Query.Domain.Entities.DeliveryEntity entity)
    {
        var proto = new DeliveryEntity
        {
            Id = entity.Id.ToString(),
            RouteId = entity.RouteId.ToString(),
            Status = entity.Status ?? string.Empty,
            DriverId = entity.DriverId?.ToString() ?? string.Empty,
            VehicleId = entity.VehicleId?.ToString() ?? string.Empty,
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc))
        };

        if (entity.DeliveredAt.HasValue)
            proto.DeliveredAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.DeliveredAt.Value, DateTimeKind.Utc));
        
        if (!string.IsNullOrEmpty(entity.SignatureUrl))
            proto.SignatureUrl = entity.SignatureUrl;
        
        if (!string.IsNullOrEmpty(entity.ReceiverName))
            proto.ReceiverName = entity.ReceiverName;

        if (entity.Incidents != null)
        {
            proto.Incidents.AddRange(entity.Incidents.Select(i => new IncidentEntity
            {
                Id = i.Id.ToString(),
                DeliveryId = i.DeliveryId.ToString(),
                Type = i.Type ?? string.Empty,
                Description = i.Description ?? string.Empty,
                ReportedTimestamp = Timestamp.FromDateTime(DateTime.SpecifyKind(i.ReportedTimestamp, DateTimeKind.Utc)),
                ResolvedTimestamp = i.ResolvedTimestamp.HasValue 
                    ? Timestamp.FromDateTime(DateTime.SpecifyKind(i.ResolvedTimestamp.Value, DateTimeKind.Utc))
                    : null,
                Resolved = i.Resolved
            }));
        }

        return proto;
    }
}
