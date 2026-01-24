using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Infrastructure;
using Eroad.FleetManagement.Common;
using Eroad.FleetManagement.Query.API.DTOs;
using Eroad.FleetManagement.Query.API.Queries;
using Eroad.FleetManagement.Query.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.FleetManagement.Query.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DriverLookupController : ControllerBase
    {
        private readonly ILogger<DriverLookupController> _logger;
        private readonly IQueryDispatcher<DriverEntity> _queryDispatcher;

        public DriverLookupController(ILogger<DriverLookupController> logger, IQueryDispatcher<DriverEntity> queryDispatcher)
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllDriversAsync()
        {
            try
            {
                var drivers = await _queryDispatcher.SendAsync(new FindAllDriversQuery());

                if (drivers == null || !drivers.Any())
                    return NoContent();

                var count = drivers.Count;
                return Ok(new DriverLookupResponse
                {
                    Drivers = drivers,
                    Message = $"Successfully returned {count} driver{(count > 1 ? "s" : "")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve all drivers!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byId/{id}")]
        public async Task<ActionResult> GetDriverByIdAsync(Guid id)
        {
            try
            {
                var drivers = await _queryDispatcher.SendAsync(new FindDriverByIdQuery { Id = id });

                if (drivers == null || !drivers.Any())
                    return NoContent();

                return Ok(new DriverLookupResponse
                {
                    Drivers = drivers,
                    Message = "Successfully returned driver!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find driver by ID!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byStatus/{status}")]
        public async Task<ActionResult> GetDriversByStatusAsync(string status)
        {
            try
            {
                var drivers = await _queryDispatcher.SendAsync(new FindDriversByStatusQuery { Status = status });

                if (drivers == null || !drivers.Any())
                    return NoContent();

                var count = drivers.Count;
                return Ok(new DriverLookupResponse
                {
                    Drivers = drivers,
                    Message = $"Successfully returned {count} driver{(count > 1 ? "s" : "")} with status '{status}'!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find drivers by status!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("statuses")]
        public ActionResult GetAllDriverStatuses()
        {
            try
            {
                var statuses = Enum.GetValues(typeof(DriverStatus))
                    .Cast<DriverStatus>()
                    .Select(s => new StatusInfo
                    {
                        Name = s.ToString(),
                        Value = (int)s
                    })
                    .ToList();

                return Ok(new DriverStatusListResponse
                {
                    Statuses = statuses,
                    Message = $"Successfully returned {statuses.Count} driver status{(statuses.Count > 1 ? "es" : "")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve driver statuses!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
