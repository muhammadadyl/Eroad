using Eroad.DeliveryTracking.Contracts;
using Eroad.DeliveryTracking.Query.API.Queries;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Services.Grpc;

public class IncidentLookupGrpcService : IncidentLookup.IncidentLookupBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IncidentLookupGrpcService> _logger;

    public IncidentLookupGrpcService(IMediator mediator, ILogger<IncidentLookupGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<IncidentLookupResponse> GetIncidentsByDelivery(GetIncidentsByDeliveryRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.DeliveryId, out var deliveryId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid delivery ID format"));
            }

            var incidents = await _mediator.Send(new FindIncidentsByDeliveryIdQuery { DeliveryId = deliveryId }, context.CancellationToken);
            return new IncidentLookupResponse
            {
                Message = "Incidents retrieved successfully",
                Incidents = { incidents.Select(MapToProto) }
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents by delivery: {DeliveryId}", request.DeliveryId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving incidents"));
        }
    }

    public override async Task<IncidentLookupResponse> GetUnresolvedIncidents(GetUnresolvedIncidentsRequest request, ServerCallContext context)
    {
        try
        {
            var incidents = await _mediator.Send(new FindAllUnresolvedIncidentsQuery(), context.CancellationToken);
            return new IncidentLookupResponse
            {
                Message = "Unresolved incidents retrieved successfully",
                Incidents = { incidents.Select(MapToProto) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unresolved incidents");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving incidents"));
        }
    }

    private static IncidentEntity MapToProto(Query.Domain.Entities.IncidentEntity entity)
    {
        return new IncidentEntity
        {
            Id = entity.Id.ToString(),
            DeliveryId = entity.DeliveryId.ToString(),
            Type = entity.Type ?? string.Empty,
            Description = entity.Description ?? string.Empty,
            ReportedTimestamp = Timestamp.FromDateTime(DateTime.SpecifyKind(entity.ReportedTimestamp, DateTimeKind.Utc)),
            ResolvedTimestamp = entity.ResolvedTimestamp.HasValue
                ? Timestamp.FromDateTime(DateTime.SpecifyKind(entity.ResolvedTimestamp.Value, DateTimeKind.Utc))
                : null,
            Resolved = entity.Resolved
        };
    }
}
