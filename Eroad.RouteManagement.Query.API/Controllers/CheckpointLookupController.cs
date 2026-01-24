using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.RouteManagement.Query.API.DTOs;
using Eroad.RouteManagement.Query.API.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.RouteManagement.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CheckpointLookupController : ControllerBase
    {
        private readonly ILogger<CheckpointLookupController> _logger;
        private readonly IQueryDispatcher<CheckpointEntity> _queryDispatcher;

        public CheckpointLookupController(ILogger<CheckpointLookupController> logger, IQueryDispatcher<CheckpointEntity> queryDispatcher)
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet("byRoute/{routeId}")]
        public async Task<ActionResult> GetCheckpointsByRouteAsync(Guid routeId)
        {
            try
            {
                var checkpoints = await _queryDispatcher.SendAsync(new FindCheckpointsByRouteIdQuery { RouteId = routeId });

                if (checkpoints == null || !checkpoints.Any())
                    return NoContent();

                var count = checkpoints.Count;
                return Ok(new CheckpointLookupResponse
                {
                    Checkpoints = checkpoints,
                    Message = $"Successfully returned {count} checkpoint{(count > 1 ? "s" : "")} for route!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find checkpoints by route!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
