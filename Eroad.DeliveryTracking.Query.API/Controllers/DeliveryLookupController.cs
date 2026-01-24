using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.DeliveryTracking.Common;
using Eroad.DeliveryTracking.Query.API.DTOs;
using Eroad.DeliveryTracking.Query.API.Queries;
using Eroad.DeliveryTracking.Query.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.DeliveryTracking.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeliveryLookupController : ControllerBase
    {
        private readonly ILogger<DeliveryLookupController> _logger;
        private readonly IQueryDispatcher<DeliveryEntity> _queryDispatcher;

        public DeliveryLookupController(ILogger<DeliveryLookupController> logger, IQueryDispatcher<DeliveryEntity> queryDispatcher)
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllDeliveriesAsync()
        {
            try
            {
                var deliveries = await _queryDispatcher.SendAsync(new FindAllDeliveriesQuery());

                if (deliveries == null || !deliveries.Any())
                    return NoContent();

                var count = deliveries.Count;
                return Ok(new DeliveryLookupResponse
                {
                    Deliveries = deliveries,
                    Message = $"Successfully returned {count} deliver{(count > 1 ? "ies" : "y")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve all deliveries!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byId/{id}")]
        public async Task<ActionResult> GetDeliveryByIdAsync(Guid id)
        {
            try
            {
                var deliveries = await _queryDispatcher.SendAsync(new FindDeliveryByIdQuery { Id = id });

                if (deliveries == null || !deliveries.Any())
                    return NoContent();

                return Ok(new DeliveryLookupResponse
                {
                    Deliveries = deliveries,
                    Message = "Successfully returned delivery!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find delivery by ID!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byStatus/{status}")]
        public async Task<ActionResult> GetDeliveriesByStatusAsync(string status)
        {
            try
            {
                var deliveries = await _queryDispatcher.SendAsync(new FindDeliveriesByStatusQuery { Status = status });

                if (deliveries == null || !deliveries.Any())
                    return NoContent();

                var count = deliveries.Count;
                return Ok(new DeliveryLookupResponse
                {
                    Deliveries = deliveries,
                    Message = $"Successfully returned {count} deliver{(count > 1 ? "ies" : "y")} with status '{status}'!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find deliveries by status!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byRoute/{routeId}")]
        public async Task<ActionResult> GetDeliveriesByRouteAsync(Guid routeId)
        {
            try
            {
                var deliveries = await _queryDispatcher.SendAsync(new FindDeliveriesByRouteIdQuery { RouteId = routeId });

                if (deliveries == null || !deliveries.Any())
                    return NoContent();

                var count = deliveries.Count;
                return Ok(new DeliveryLookupResponse
                {
                    Deliveries = deliveries,
                    Message = $"Successfully returned {count} deliver{(count > 1 ? "ies" : "y")} for route!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find deliveries by route!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("statuses")]
        public ActionResult GetAllDeliveryStatuses()
        {
            try
            {
                var statuses = Enum.GetValues(typeof(DeliveryStatus))
                    .Cast<DeliveryStatus>()
                    .Select(s => new StatusInfo
                    {
                        Name = s.ToString(),
                        Value = (int)s
                    })
                    .ToList();

                return Ok(new StatusLookupResponse
                {
                    Statuses = statuses,
                    Message = $"Successfully returned {statuses.Count} delivery statuses!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve delivery statuses!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
