using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindDeliveriesByRouteIdQueryHandler : IRequestHandler<FindDeliveriesByRouteIdQuery, List<DeliveryEntity>>
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public FindDeliveriesByRouteIdQueryHandler(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<List<DeliveryEntity>> Handle(FindDeliveriesByRouteIdQuery request, CancellationToken cancellationToken)
        {
            return await _deliveryRepository.GetByRouteIdAsync(request.RouteId);
        }
    }
}
