using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.DeliveryTracking.Query.API.DTOs;
using Eroad.DeliveryTracking.Query.API.Queries;
using Eroad.DeliveryTracking.Query.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.DeliveryTracking.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class IncidentLookupController : ControllerBase
    {
        private readonly ILogger<IncidentLookupController> _logger;
        private readonly IQueryDispatcher<IncidentEntity> _queryDispatcher;

        public IncidentLookupController(ILogger<IncidentLookupController> logger, IQueryDispatcher<IncidentEntity> queryDispatcher)
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet("byDelivery/{deliveryId}")]
        public async Task<ActionResult> GetIncidentsByDeliveryAsync(Guid deliveryId)
        {
            try
            {
                var incidents = await _queryDispatcher.SendAsync(new FindIncidentsByDeliveryIdQuery { DeliveryId = deliveryId });

                if (incidents == null || !incidents.Any())
                    return NoContent();

                var count = incidents.Count;
                return Ok(new IncidentLookupResponse
                {
                    Incidents = incidents,
                    Message = $"Successfully returned {count} incident{(count > 1 ? "s" : "")} for delivery!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find incidents by delivery!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("unresolved")]
        public async Task<ActionResult> GetAllUnresolvedIncidentsAsync()
        {
            try
            {
                var incidents = await _queryDispatcher.SendAsync(new FindAllUnresolvedIncidentsQuery());

                if (incidents == null || !incidents.Any())
                    return NoContent();

                var count = incidents.Count;
                return Ok(new IncidentLookupResponse
                {
                    Incidents = incidents,
                    Message = $"Successfully returned {count} unresolved incident{(count > 1 ? "s" : "")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve unresolved incidents!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
