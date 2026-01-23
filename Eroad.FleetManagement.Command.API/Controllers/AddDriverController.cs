using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.FleetManagement.Command.API.Commands;
using Eroad.FleetManagement.Command.API.Commands.Driver;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.FleetManagement.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AddDriverController : ControllerBase
    {
        private readonly ILogger<AddDriverController> _logger;
        private readonly IDriverCommandHandler _commandHandler;

        public AddDriverController(ILogger<AddDriverController> logger, IDriverCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPost]
        public async Task<ActionResult> AddDriverAsync(AddDriverCommand command)
        {
            try
            {
                command = command with { Id = Guid.NewGuid() };
                await _commandHandler.HandleAsync(command);

                return StatusCode(StatusCodes.Status201Created, new BaseResponse
                {
                    Message = "Add driver request completed successfully!"
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
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to add a driver!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
