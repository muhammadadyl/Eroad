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

    public override async Task<DeliveryLookupResponse> GetDeliveriesByStatus(GetDeliveriesByStatusRequest request, ServerCallContext context)
    {
        try
        {
            var deliveries = await _mediator.Send(new FindDeliveriesByStatusQuery { Status = request.Status }, context.CancellationToken);
            return new DeliveryLookupResponse
            {
                Message = "Deliveries retrieved successfully",
                Deliveries = { deliveries.Select(MapToProto) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deliveries by status: {Status}", request.Status);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving deliveries"));
        }
    }

    public override async Task<DeliveryLookupResponse> GetDeliveriesByRoute(GetDeliveriesByRouteRequest request, ServerCallContext context)
    {
        try
        {if (!Guid.TryParse(request.RouteId, out var routeId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
            }

            var deliveries = await _mediator.Send(new FindDeliveriesByRouteIdQuery { RouteId = routeId }, context.CancellationToken);
            return new DeliveryLookupResponse
            {
                Message = "Deliveries retrieved successfully",
                Deliveries = { deliveries.Select(MapToProto) }
            };
        }
        catch (RpcException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid route ID format"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deliveries by route: {RouteId}", request.RouteId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving deliveries"));
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

    public override async Task<DeliveryCheckpointsResponse> GetDeliveryCheckpoints(GetDeliveryCheckpointsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DeliveryId, out var deliveryId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
            }

            var checkpoints = await _mediator.Send(new FindDeliveryCheckpointsQuery { DeliveryId = deliveryId }, context.CancellationToken);

            var response = new DeliveryCheckpointsResponse
            {
                Message = $"Found {checkpoints.Count} checkpoint(s)",
                RouteId = checkpoints.FirstOrDefault()?.RouteId.ToString() ?? string.Empty
            };

            foreach (var checkpoint in checkpoints)
            {
                response.Checkpoints.Add(new DeliveryCheckpointEntity
                {
                    DeliveryId = checkpoint.DeliveryId.ToString(),
                    Sequence = checkpoint.Sequence,
                    RouteId = checkpoint.RouteId.ToString(),
                    Location = checkpoint.Location,
                    ReachedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(checkpoint.ReachedAt, DateTimeKind.Utc))
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
            _logger.LogError(ex, "Error retrieving delivery checkpoints for delivery {DeliveryId}", request.DeliveryId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving checkpoints"));
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

    private static DeliveryEntity MapToProto(Query.Domain.Entities.DeliveryEntity entity)
    {
        var proto = new DeliveryEntity
        {
            Id = entity.Id.ToString(),
            RouteId = entity.RouteId.ToString(),
            Status = entity.Status ?? string.Empty,
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc))
        };

        if (entity.DeliveredAt.HasValue)
            proto.DeliveredAt = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.DeliveredAt.Value, DateTimeKind.Utc));
        
        if (!string.IsNullOrEmpty(entity.SignatureUrl))
            proto.SignatureUrl = entity.SignatureUrl;
        
        if (!string.IsNullOrEmpty(entity.ReceiverName))
            proto.ReceiverName = entity.ReceiverName;
        
        if (!string.IsNullOrEmpty(entity.CurrentCheckpoint))
            proto.CurrentCheckpoint = entity.CurrentCheckpoint;

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
