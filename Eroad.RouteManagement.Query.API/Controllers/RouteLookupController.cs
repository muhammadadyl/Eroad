using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.RouteManagement.Common;
using Eroad.RouteManagement.Query.API.DTOs;
using Eroad.RouteManagement.Query.API.Queries;
using Eroad.RouteManagement.Query.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.RouteManagement.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RouteLookupController : ControllerBase
    {
        private readonly ILogger<RouteLookupController> _logger;
        private readonly IQueryDispatcher<RouteEntity> _queryDispatcher;

        public RouteLookupController(ILogger<RouteLookupController> logger, IQueryDispatcher<RouteEntity> queryDispatcher)
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllRoutesAsync()
        {
            try
            {
                var routes = await _queryDispatcher.SendAsync(new FindAllRoutesQuery());

                if (routes == null || !routes.Any())
                    return NoContent();

                var count = routes.Count;
                return Ok(new RouteLookupResponse
                {
                    Routes = routes,
                    Message = $"Successfully returned {count} route{(count > 1 ? "s" : "")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve all routes!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byId/{id}")]
        public async Task<ActionResult> GetRouteByIdAsync(Guid id)
        {
            try
            {
                var routes = await _queryDispatcher.SendAsync(new FindRouteByIdQuery { Id = id });

                if (routes == null || !routes.Any())
                    return NoContent();

                return Ok(new RouteLookupResponse
                {
                    Routes = routes,
                    Message = "Successfully returned route!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find route by ID!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byStatus/{status}")]
        public async Task<ActionResult> GetRoutesByStatusAsync(string status)
        {
            try
            {
                var routes = await _queryDispatcher.SendAsync(new FindRoutesByStatusQuery { Status = status });

                if (routes == null || !routes.Any())
                    return NoContent();

                var count = routes.Count;
                return Ok(new RouteLookupResponse
                {
                    Routes = routes,
                    Message = $"Successfully returned {count} route{(count > 1 ? "s" : "")} with status '{status}'!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find routes by status!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byDriver/{driverId}")]
        public async Task<ActionResult> GetRoutesByDriverAsync(Guid driverId)
        {
            try
            {
                var routes = await _queryDispatcher.SendAsync(new FindRoutesByDriverIdQuery { DriverId = driverId });

                if (routes == null || !routes.Any())
                    return NoContent();

                var count = routes.Count;
                return Ok(new RouteLookupResponse
                {
                    Routes = routes,
                    Message = $"Successfully returned {count} route{(count > 1 ? "s" : "")} for driver!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find routes by driver!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byVehicle/{vehicleId}")]
        public async Task<ActionResult> GetRoutesByVehicleAsync(Guid vehicleId)
        {
            try
            {
                var routes = await _queryDispatcher.SendAsync(new FindRoutesByVehicleIdQuery { VehicleId = vehicleId });

                if (routes == null || !routes.Any())
                    return NoContent();

                var count = routes.Count;
                return Ok(new RouteLookupResponse
                {
                    Routes = routes,
                    Message = $"Successfully returned {count} route{(count > 1 ? "s" : "")} for vehicle!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find routes by vehicle!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("statuses")]
        public ActionResult GetAllRouteStatuses()
        {
            try
            {
                var statuses = Enum.GetValues(typeof(RouteStatus))
                    .Cast<RouteStatus>()
                    .Select(s => new StatusInfo
                    {
                        Name = s.ToString(),
                        Value = (int)s
                    })
                    .ToList();

                return Ok(new StatusLookupResponse
                {
                    Statuses = statuses,
                    Message = $"Successfully returned {statuses.Count} route statuses!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve route statuses!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
