using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Queries.Handlers
{
    public class FindVehiclesByStatusQueryHandler : IRequestHandler<FindVehiclesByStatusQuery, List<VehicleEntity>>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public FindVehiclesByStatusQueryHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<List<VehicleEntity>> Handle(FindVehiclesByStatusQuery request, CancellationToken cancellationToken)
        {
            return await _vehicleRepository.GetByStatusAsync(request.Status);
        }
    }
}
