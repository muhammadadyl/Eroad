using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.FleetManagement.Command.API.Commands;
using Eroad.FleetManagement.Command.API.Commands.Driver;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.FleetManagement.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UpdateDriverController : ControllerBase
    {
        private readonly ILogger<UpdateDriverController> _logger;
        private readonly IDriverCommandHandler _commandHandler;

        public UpdateDriverController(ILogger<UpdateDriverController> logger, IDriverCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDriverAsync(Guid id, UpdateDriverCommand command)
        {
            try
            {
                command = command with { Id = id };
                await _commandHandler.HandleAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Update driver request completed successfully!"
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
                _logger.Log(LogLevel.Warning, ex, "Could not retrieve aggregate, client passed an incorrect driver ID targetting the aggregate!");
                return BadRequest(new BaseResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to update a driver!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
