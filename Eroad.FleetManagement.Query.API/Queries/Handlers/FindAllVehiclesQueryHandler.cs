using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Queries.Handlers
{
    public class FindAllVehiclesQueryHandler : IRequestHandler<FindAllVehiclesQuery, List<VehicleEntity>>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public FindAllVehiclesQueryHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<List<VehicleEntity>> Handle(FindAllVehiclesQuery request, CancellationToken cancellationToken)
        {
            return await _vehicleRepository.GetAllAsync();
        }
    }
}
