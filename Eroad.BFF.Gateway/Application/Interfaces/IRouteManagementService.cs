using Eroad.BFF.Gateway.Application.Models;

namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface IRouteManagementService
{
    // Query Operations
    Task<RouteOverviewView> GetRouteOverviewAsync();
    Task<RouteDetailView> GetRouteDetailAsync(Guid routeId);

    // Command Operations
    Task<object> CreateRouteAsync(string id, string origin, string destination);
    Task<object> UpdateRouteAsync(string id, string origin, string destination);
    Task<object> ChangeRouteStatusAsync(string id, string status);
    Task<object> AddCheckpointAsync(string id, int sequence, string location, DateTime expectedTime);
}
