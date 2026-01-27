using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindActiveDeliveriesByVehicleQueryHandler : IRequestHandler<FindActiveDeliveriesByVehicleQuery, List<DeliveryEntity>>
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public FindActiveDeliveriesByVehicleQueryHandler(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<List<DeliveryEntity>> Handle(FindActiveDeliveriesByVehicleQuery request, CancellationToken cancellationToken)
        {
            return await _deliveryRepository.GetActiveDeliveriesByVehicleAsync(request.VehicleId);
        }
    }
}
