using Eroad.DeliveryTracking.Query.Domain.Entities;
using Eroad.DeliveryTracking.Query.Domain.Repositories;
using MediatR;

namespace Eroad.DeliveryTracking.Query.API.Queries.Handlers
{
    public class FindDeliveryByIdQueryHandler : IRequestHandler<FindDeliveryByIdQuery, List<DeliveryEntity>>
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public FindDeliveryByIdQueryHandler(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<List<DeliveryEntity>> Handle(FindDeliveryByIdQuery request, CancellationToken cancellationToken)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(request.Id);
            return delivery != null ? new List<DeliveryEntity> { delivery } : new List<DeliveryEntity>();
        }
    }
}
