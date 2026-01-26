using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindDeliveriesByStatusQueryHandler : IRequestHandler<FindDeliveriesByStatusQuery, List<DeliveryEntity>>
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public FindDeliveriesByStatusQueryHandler(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<List<DeliveryEntity>> Handle(FindDeliveriesByStatusQuery request, CancellationToken cancellationToken)
        {
            return await _deliveryRepository.GetByStatusAsync(request.Status);
        }
    }
}
