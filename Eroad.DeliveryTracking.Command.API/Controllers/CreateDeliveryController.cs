using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.DeliveryTracking.Command.API.Commands.Delivery;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.DeliveryTracking.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CreateDeliveryController : ControllerBase
    {
        private readonly ILogger<CreateDeliveryController> _logger;
        private readonly IDeliveryCommandHandler _commandHandler;

        public CreateDeliveryController(ILogger<CreateDeliveryController> logger, IDeliveryCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPost]
        public async Task<ActionResult> CreateDeliveryAsync(CreateDeliveryCommand command)
        {
            try
            {
                command = command with { Id = Guid.NewGuid() };
                await _commandHandler.HandleAsync(command);

                return StatusCode(StatusCodes.Status201Created, new BaseResponse
                {
                    Message = "Create delivery request completed successfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to create a delivery!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
