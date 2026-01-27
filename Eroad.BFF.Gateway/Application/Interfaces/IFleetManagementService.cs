using Eroad.BFF.Gateway.Application.Models;

namespace Eroad.BFF.Gateway.Application.Interfaces;

public interface IFleetManagementService
{
    // Query Operations
    Task<VehicleDetailView> GetVehicleDetailAsync(Guid vehicleId);
    Task<DriverDetailView> GetDriverDetailAsync(Guid driverId);

    // Vehicle Command Operations
    Task<object> AddVehicleAsync(string id, string registration, string vehicleType);
    Task<object> UpdateVehicleAsync(string id, string registration, string vehicleType);
    Task<object> ChangeVehicleStatusAsync(string id, string status);

    // Driver Command Operations
    Task<object> AddDriverAsync(string id, string name, string driverLicense);
    Task<object> UpdateDriverAsync(string id, string name, string driverLicense);
    Task<object> ChangeDriverStatusAsync(string id, string status);
}
