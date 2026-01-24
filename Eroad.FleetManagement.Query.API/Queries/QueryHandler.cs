using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;

namespace Eroad.FleetManagement.Query.API.Queries
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public QueryHandler(IDriverRepository driverRepository, IVehicleRepository vehicleRepository)
        {
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<List<DriverEntity>> HandleAsync(FindAllDriversQuery query)
        {
            return await _driverRepository.GetAllAsync();
        }

        public async Task<List<DriverEntity>> HandleAsync(FindDriverByIdQuery query)
        {
            var driver = await _driverRepository.GetByIdAsync(query.Id);
            return driver != null ? new List<DriverEntity> { driver } : new List<DriverEntity>();
        }

        public async Task<List<VehicleEntity>> HandleAsync(FindAllVehiclesQuery query)
        {
            return await _vehicleRepository.GetAllAsync();
        }

        public async Task<List<VehicleEntity>> HandleAsync(FindVehicleByIdQuery query)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(query.Id);
            return vehicle != null ? new List<VehicleEntity> { vehicle } : new List<VehicleEntity>();
        }

        public async Task<List<VehicleEntity>> HandleAsync(FindVehicleByDriverIdQuery query)
        {
            return await _vehicleRepository.GetByDriverIdAsync(query.DriverId);
        }

        public async Task<List<DriverEntity>> HandleAsync(FindDriversByStatusQuery query)
        {
            return await _driverRepository.GetByStatusAsync(query.Status);
        }

        public async Task<List<VehicleEntity>> HandleAsync(FindVehiclesByStatusQuery query)
        {
            return await _vehicleRepository.GetByStatusAsync(query.Status);
        }
    }
}
