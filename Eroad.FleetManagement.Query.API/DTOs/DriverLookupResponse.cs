using Eroad.Common.DTOs;
using Eroad.FleetManagement.Query.Domain.Entities;

namespace Eroad.FleetManagement.Query.API.DTOs
{
    public class DriverLookupResponse : BaseResponse
    {
        public required List<DriverEntity> Drivers { get; set; }
    }
}
