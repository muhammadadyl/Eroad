using Eroad.BFF.Gateway.Application.DTOs;

namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface IFleetManagementService
{
    Task<FleetOverviewView> GetFleetOverviewAsync();
    Task<VehicleDetailView> GetVehicleDetailAsync(Guid vehicleId);
    Task<DriverDetailView> GetDriverDetailAsync(Guid driverId);
}
