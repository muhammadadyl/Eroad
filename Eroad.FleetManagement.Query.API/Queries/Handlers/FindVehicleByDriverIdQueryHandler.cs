using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Queries.Handlers
{
    public class FindVehicleByDriverIdQueryHandler : IRequestHandler<FindVehicleByDriverIdQuery, List<VehicleEntity>>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public FindVehicleByDriverIdQueryHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<List<VehicleEntity>> Handle(FindVehicleByDriverIdQuery request, CancellationToken cancellationToken)
        {
            return await _vehicleRepository.GetByDriverIdAsync(request.DriverId);
        }
    }
}
