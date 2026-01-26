using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Queries.Handlers
{
    public class FindVehicleByIdQueryHandler : IRequestHandler<FindVehicleByIdQuery, List<VehicleEntity>>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public FindVehicleByIdQueryHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<List<VehicleEntity>> Handle(FindVehicleByIdQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.Id);
            return vehicle != null ? new List<VehicleEntity> { vehicle } : new List<VehicleEntity>();
        }
    }
}
