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
    public class VehicleLookupController : ControllerBase
    {
        private readonly ILogger<VehicleLookupController> _logger;
        private readonly IQueryDispatcher<VehicleEntity> _queryDispatcher;

        public VehicleLookupController(ILogger<VehicleLookupController> logger, IQueryDispatcher<VehicleEntity> queryDispatcher)
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllVehiclesAsync()
        {
            try
            {
                var vehicles = await _queryDispatcher.SendAsync(new FindAllVehiclesQuery());

                if (vehicles == null || !vehicles.Any())
                    return NoContent();

                var count = vehicles.Count;
                return Ok(new VehicleLookupResponse
                {
                    Vehicles = vehicles,
                    Message = $"Successfully returned {count} vehicle{(count > 1 ? "s" : "")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve all vehicles!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byId/{id}")]
        public async Task<ActionResult> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                var vehicles = await _queryDispatcher.SendAsync(new FindVehicleByIdQuery { Id = id });

                if (vehicles == null || !vehicles.Any())
                    return NoContent();

                return Ok(new VehicleLookupResponse
                {
                    Vehicles = vehicles,
                    Message = "Successfully returned vehicle!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find vehicle by ID!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byDriver/{driverId}")]
        public async Task<ActionResult> GetVehiclesByDriverAsync(Guid driverId)
        {
            try
            {
                var vehicles = await _queryDispatcher.SendAsync(new FindVehicleByDriverIdQuery { DriverId = driverId });

                if (vehicles == null || !vehicles.Any())
                    return NoContent();

                var count = vehicles.Count;
                return Ok(new VehicleLookupResponse
                {
                    Vehicles = vehicles,
                    Message = $"Successfully returned {count} vehicle{(count > 1 ? "s" : "")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find vehicles by driver!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("byStatus/{status}")]
        public async Task<ActionResult> GetVehiclesByStatusAsync(string status)
        {
            try
            {
                var vehicles = await _queryDispatcher.SendAsync(new FindVehiclesByStatusQuery { Status = status });

                if (vehicles == null || !vehicles.Any())
                    return NoContent();

                var count = vehicles.Count;
                return Ok(new VehicleLookupResponse
                {
                    Vehicles = vehicles,
                    Message = $"Successfully returned {count} vehicle{(count > 1 ? "s" : "")} with status '{status}'!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to find vehicles by status!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }

        [HttpGet("statuses")]
        public ActionResult GetAllVehicleStatuses()
        {
            try
            {
                var statuses = Enum.GetValues(typeof(VehicleStatus))
                    .Cast<VehicleStatus>()
                    .Select(s => new StatusInfo
                    {
                        Name = s.ToString(),
                        Value = (int)s
                    })
                    .ToList();

                return Ok(new VehicleStatusListResponse
                {
                    Statuses = statuses,
                    Message = $"Successfully returned {statuses.Count} vehicle status{(statuses.Count > 1 ? "es" : "")}!"
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve vehicle statuses!";
                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
