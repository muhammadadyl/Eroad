using Eroad.Common.DTOs;
using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.DTOs
{
    public class CheckpointLookupResponse : BaseResponse
    {
        public required List<CheckpointEntity> Checkpoints { get; set; }
    }
}
