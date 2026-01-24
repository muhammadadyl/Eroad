using Eroad.FleetManagement.Common;
using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;

namespace Eroad.FleetManagement.Query.Infrastructure.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public EventHandler(IDriverRepository driverRepository, IVehicleRepository vehicleRepository)
        {
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task On(DriverAddedEvent @event)
        {
            var driver = new DriverEntity
            {
                Id = @event.Id,
                Name = @event.Name,
                DriverLicense = @event.DriverLicence,
                Status = @event.DriverStatus.ToString()
            };

            await _driverRepository.CreateAsync(driver);
        }

        public async Task On(DriverUpdatedEvent @event)
        {
            var driver = await _driverRepository.GetByIdAsync(@event.Id);

            if (driver == null) return;

            driver.DriverLicense = @event.DriverLicence;

            await _driverRepository.UpdateAsync(driver);
        }

        public async Task On(DriverStatusChangedEvent @event)
        {
            var driver = await _driverRepository.GetByIdAsync(@event.Id);

            if (driver == null) return;

            driver.Status = @event.NewStatus.ToString();

            await _driverRepository.UpdateAsync(driver);
        }

        public async Task On(VehicleAddedEvent @event)
        {
            var vehicle = new VehicleEntity
            {
                Id = @event.Id,
                Registration = @event.Registration,
                VehicleType = @event.VehicleType,
                Status = @event.VehicleStatus.ToString()
            };

            await _vehicleRepository.CreateAsync(vehicle);
        }

        public async Task On(VehicleUpdatedEvent @event)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(@event.Id);

            if (vehicle == null) return;

            vehicle.Registration = @event.Registration;
            vehicle.VehicleType = @event.VehicleType;

            await _vehicleRepository.UpdateAsync(vehicle);
        }

        public async Task On(VehicleStatusChangedEvent @event)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(@event.Id);

            if (vehicle == null) return;

            vehicle.Status = @event.NewStatus.ToString();

            await _vehicleRepository.UpdateAsync(vehicle);
        }

        public async Task On(DriverAssignedEvent @event)
        {
            var driver = await _driverRepository.GetByIdAsync(@event.DriverId);

            if (driver == null) return;

            driver.AssignedVehicleId = @event.Id;

            await _driverRepository.UpdateAsync(driver);

            var vehicle = await _vehicleRepository.GetByIdAsync(@event.Id);

            if (vehicle == null) return;

            vehicle.AssignedDriverId = @event.DriverId;

            await _vehicleRepository.UpdateAsync(vehicle);
        }
    }
}
