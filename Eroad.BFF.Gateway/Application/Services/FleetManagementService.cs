using Eroad.BFF.Gateway.Application.Interfaces;
using Eroad.FleetManagement.Contracts;

namespace Eroad.BFF.Gateway.Application.Services;

public class FleetManagementService : IFleetManagementService
{
    private readonly DriverLookup.DriverLookupClient _driverClient;
    private readonly VehicleLookup.VehicleLookupClient _vehicleClient;
    private readonly VehicleCommand.VehicleCommandClient _vehicleCommandClient;
    private readonly DriverCommand.DriverCommandClient _driverCommandClient;
    private readonly ILogger<FleetManagementService> _logger;

    public FleetManagementService(
        DriverLookup.DriverLookupClient driverClient,
        VehicleLookup.VehicleLookupClient vehicleClient,
        VehicleCommand.VehicleCommandClient vehicleCommandClient,
        DriverCommand.DriverCommandClient driverCommandClient,
        ILogger<FleetManagementService> logger)
    {
        _driverClient = driverClient;
        _vehicleClient = vehicleClient;
        _vehicleCommandClient = vehicleCommandClient;
        _driverCommandClient = driverCommandClient;
        _logger = logger;
    }

    public async Task<VehicleDetailViewModel> GetVehicleDetailAsync(Guid vehicleId)
    {
        _logger.LogInformation("Fetching vehicle detail for ID: {VehicleId}", vehicleId);

        var vehicleResponse = await _vehicleClient.GetVehicleByIdAsync(new GetVehicleByIdRequest { Id = vehicleId.ToString() });
        var vehicle = vehicleResponse.Vehicles.FirstOrDefault();

        if (vehicle == null)
        {
            throw new InvalidOperationException($"Vehicle with ID {vehicleId} not found");
        }

        return new VehicleDetailViewModel
        {
            VehicleId = Guid.Parse(vehicle.Id),
            Registration = vehicle.Registration,
            VehicleType = vehicle.VehicleType,
            Status = vehicle.Status
        };
    }

    public async Task<DriverDetailViewModel> GetDriverDetailAsync(Guid driverId)
    {
        _logger.LogInformation("Fetching driver detail for ID: {DriverId}", driverId);

        var driverResponse = await _driverClient.GetDriverByIdAsync(new GetDriverByIdRequest { Id = driverId.ToString() });
        var driver = driverResponse.Drivers.FirstOrDefault();

        if (driver == null)
        {
            throw new InvalidOperationException($"Driver with ID {driverId} not found");
        }

        return new DriverDetailViewModel
        {
            DriverId = Guid.Parse(driver.Id),
            Name = driver.Name,
            DriverLicense = driver.DriverLicense,
            Status = driver.Status
        };
    }

    public async Task<object> AddVehicleAsync(string id, string registration, string vehicleType)
    {
        _logger.LogInformation("Adding new vehicle with registration: {Registration}", registration);
        var request = new AddVehicleRequest
        {
            Id = id,
            Registration = registration,
            VehicleType = vehicleType
        };
        var response = await _vehicleCommandClient.AddVehicleAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> UpdateVehicleAsync(string id, string registration, string vehicleType)
    {
        _logger.LogInformation("Updating vehicle: {VehicleId}", id);
        var request = new UpdateVehicleRequest
        {
            Id = id,
            Registration = registration,
            VehicleType = vehicleType
        };
        var response = await _vehicleCommandClient.UpdateVehicleAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> ChangeVehicleStatusAsync(string id, string status)
    {
        _logger.LogInformation("Changing vehicle status: {VehicleId} to {Status}", id, status);
        var request = new ChangeVehicleStatusRequest
        {
            Id = id,
            Status = status
        };
        var response = await _vehicleCommandClient.ChangeVehicleStatusAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> AddDriverAsync(string id, string name, string driverLicense)
    {
        _logger.LogInformation("Adding new driver: {Name}", name);
        var request = new AddDriverRequest
        {
            Id = id,
            Name = name,
            DriverLicense = driverLicense
        };
        var response = await _driverCommandClient.AddDriverAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> UpdateDriverAsync(string id, string name, string driverLicense)
    {
        _logger.LogInformation("Updating driver: {DriverId}", id);
        var request = new UpdateDriverRequest
        {
            Id = id,
            Name = name,
            DriverLicense = driverLicense
        };
        var response = await _driverCommandClient.UpdateDriverAsync(request);
        return new { Message = response.Message };
    }

    public async Task<object> ChangeDriverStatusAsync(string id, string status)
    {
        _logger.LogInformation("Changing driver status: {DriverId} to {Status}", id, status);
        var request = new ChangeDriverStatusRequest
        {
            Id = id,
            Status = status
        };
        var response = await _driverCommandClient.ChangeDriverStatusAsync(request);
        return new { Message = response.Message };
    }
}
