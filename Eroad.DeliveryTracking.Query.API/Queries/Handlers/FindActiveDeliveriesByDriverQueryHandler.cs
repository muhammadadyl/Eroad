using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindActiveDeliveriesByDriverQueryHandler : IRequestHandler<FindActiveDeliveriesByDriverQuery, List<DeliveryEntity>>
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public FindActiveDeliveriesByDriverQueryHandler(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<List<DeliveryEntity>> Handle(FindActiveDeliveriesByDriverQuery request, CancellationToken cancellationToken)
        {
            return await _deliveryRepository.GetActiveDeliveriesByDriverAsync(request.DriverId);
        }
    }
}
