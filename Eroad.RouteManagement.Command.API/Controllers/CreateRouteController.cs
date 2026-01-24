using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.RouteManagement.Command.API.Commands;
using Eroad.RouteManagement.Command.API.Commands.Route;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.RouteManagement.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CreateRouteController : ControllerBase
    {
        private readonly ILogger<CreateRouteController> _logger;
        private readonly IRouteCommandHandler _commandHandler;

        public CreateRouteController(ILogger<CreateRouteController> logger, IRouteCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPost]
        public async Task<ActionResult> CreateRouteAsync(CreateRouteCommand command)
        {
            try
            {
                await _commandHandler.HandleAsync(command);

                return StatusCode(StatusCodes.Status201Created, new BaseResponse
                {
                    Message = "Create route request completed successfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to create a route!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
