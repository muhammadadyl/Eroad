using Eroad.BFF.Gateway.Application.DTOs;

namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface IRouteManagementService
{
    Task<RouteOverviewView> GetRouteOverviewAsync();
    Task<RouteDetailView> GetRouteDetailAsync(Guid routeId);
}
