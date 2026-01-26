using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Queries.Handlers
{
    public class FindAllDriversQueryHandler : IRequestHandler<FindAllDriversQuery, List<DriverEntity>>
    {
        private readonly IDriverRepository _driverRepository;

        public FindAllDriversQueryHandler(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<List<DriverEntity>> Handle(FindAllDriversQuery request, CancellationToken cancellationToken)
        {
            return await _driverRepository.GetAllAsync();
        }
    }
}
