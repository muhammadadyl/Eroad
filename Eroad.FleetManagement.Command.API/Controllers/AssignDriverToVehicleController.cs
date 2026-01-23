using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.FleetManagement.Command.API.Commands;
using Eroad.FleetManagement.Command.API.Commands.Vehicle;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.FleetManagement.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssignDriverToVehicleController : ControllerBase
    {
        private readonly ILogger<AssignDriverToVehicleController> _logger;
        private readonly IVehicleCommandHandler _commandHandler;

        public AssignDriverToVehicleController(ILogger<AssignDriverToVehicleController> logger, IVehicleCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPut]
        public async Task<ActionResult> AssignDriverToVehicleAsync(AssignDriverToVehicleCommand command)
        {
            try
            {
                await _commandHandler.HandleAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Assign driver to vehicle request completed successfully!"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.Log(LogLevel.Warning, ex, "Client made a bad request!");
                return BadRequest(new BaseResponse
                {
                    Message = ex.Message
                });
            }
            catch (AggregateNotFoundException ex)
            {
                _logger.Log(LogLevel.Warning, ex, "Could not retrieve aggregate, client passed an incorrect vehicle or driver ID targetting the aggregate!");
                return BadRequest(new BaseResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to assign driver to vehicle!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
