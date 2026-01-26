using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Queries.Handlers
{
    public class FindDriversByStatusQueryHandler : IRequestHandler<FindDriversByStatusQuery, List<DriverEntity>>
    {
        private readonly IDriverRepository _driverRepository;

        public FindDriversByStatusQueryHandler(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<List<DriverEntity>> Handle(FindDriversByStatusQuery request, CancellationToken cancellationToken)
        {
            return await _driverRepository.GetByStatusAsync(request.Status);
        }
    }
}
