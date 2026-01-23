using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.RouteManagement.Command.API.Commands;
using Eroad.RouteManagement.Command.API.Commands.Route;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.RouteManagement.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssignVehicleToRouteController : ControllerBase
    {
        private readonly ILogger<AssignVehicleToRouteController> _logger;
        private readonly IRouteCommandHandler _commandHandler;

        public AssignVehicleToRouteController(ILogger<AssignVehicleToRouteController> logger, IRouteCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> AssignVehicleToRouteAsync(Guid id, AssignVehicleToRouteCommand command)
        {
            try
            {
                command = command with { Id = id };
                await _commandHandler.HandleAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Assign vehicle to route request completed successfully!"
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
                _logger.Log(LogLevel.Warning, ex, "Could not retrieve aggregate, client passed an incorrect route ID!");
                return BadRequest(new BaseResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to assign vehicle to route!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
