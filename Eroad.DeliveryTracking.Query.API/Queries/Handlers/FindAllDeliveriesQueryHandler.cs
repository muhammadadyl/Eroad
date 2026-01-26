using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindAllDeliveriesQueryHandler : IRequestHandler<FindAllDeliveriesQuery, List<DeliveryEntity>>
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public FindAllDeliveriesQueryHandler(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<List<DeliveryEntity>> Handle(FindAllDeliveriesQuery request, CancellationToken cancellationToken)
        {
            return await _deliveryRepository.GetAllAsync();
        }
    }
}
