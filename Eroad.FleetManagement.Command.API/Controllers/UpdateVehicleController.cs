using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.FleetManagement.Command.API.Commands;
using Eroad.FleetManagement.Command.API.Commands.Vehicle;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.FleetManagement.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UpdateVehicleController : ControllerBase
    {
        private readonly ILogger<UpdateVehicleController> _logger;
        private readonly IVehicleCommandHandler _commandHandler;

        public UpdateVehicleController(ILogger<UpdateVehicleController> logger, IVehicleCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVehicleAsync(Guid id, UpdateVehicleCommand command)
        {
            try
            {
                command.Id = id;
                await _commandHandler.HandleAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Update vehicle request completed successfully!"
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
                _logger.Log(LogLevel.Warning, ex, "Could not retrieve aggregate, client passed an incorrect vehicle ID targetting the aggregate!");
                return BadRequest(new BaseResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to update a vehicle!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
