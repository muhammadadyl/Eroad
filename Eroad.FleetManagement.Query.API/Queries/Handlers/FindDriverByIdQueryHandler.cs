using Eroad.FleetManagement.Query.Domain.Entities;
using Eroad.FleetManagement.Query.Domain.Repositories;
using MediatR;

namespace Eroad.FleetManagement.Query.API.Queries.Handlers
{
    public class FindDriverByIdQueryHandler : IRequestHandler<FindDriverByIdQuery, List<DriverEntity>>
    {
        private readonly IDriverRepository _driverRepository;

        public FindDriverByIdQueryHandler(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<List<DriverEntity>> Handle(FindDriverByIdQuery request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(request.Id);
            return driver != null ? new List<DriverEntity> { driver } : new List<DriverEntity>();
        }
    }
}
