using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.FleetManagement.Command.API.Commands;
using Eroad.FleetManagement.Command.API.Commands.Vehicle;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.FleetManagement.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AddVehicleController : ControllerBase
    {
        private readonly ILogger<AddVehicleController> _logger;
        private readonly IVehicleCommandHandler _commandHandler;

        public AddVehicleController(ILogger<AddVehicleController> logger, IVehicleCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPost]
        public async Task<ActionResult> AddVehicleAsync(AddVehicleCommand command)
        {
            try
            {
                await _commandHandler.HandleAsync(command);

                return StatusCode(StatusCodes.Status201Created, new BaseResponse
                {
                    Message = "Add vehicle request completed successfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to add a vehicle!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
