using Eroad.RouteManagement.Query.Domain.Entities;

namespace Eroad.RouteManagement.Query.API.DTOs
{
    public class CheckpointLookupResponse
    {
        public string Message { get; set; }
        public required List<CheckpointEntity> Checkpoints { get; set; }
    }
}
