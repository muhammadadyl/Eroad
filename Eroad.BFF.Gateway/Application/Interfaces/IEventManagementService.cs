using Eroad.BFF.Gateway.Application.DTOs;

namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface IEventManagementService
{
    Task<DeliveryEventsView> GetDeliveryEventsAsync(Guid deliveryId);
    Task<IncidentDashboardView> GetIncidentDashboardAsync();
    Task<IncidentTimelineView> GetIncidentTimelineAsync(Guid incidentId);
    Task<EventStatisticsView> GetEventStatisticsAsync();
}
