using Eroad.Common.DTOs;
using Eroad.CQRS.Core.Exceptions;
using Eroad.DeliveryTracking.Command.API.Commands.Delivery;
using Microsoft.AspNetCore.Mvc;

namespace Eroad.DeliveryTracking.Command.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ReportIncidentController : ControllerBase
    {
        private readonly ILogger<ReportIncidentController> _logger;
        private readonly IDeliveryCommandHandler _commandHandler;

        public ReportIncidentController(ILogger<ReportIncidentController> logger, IDeliveryCommandHandler commandHandler)
        {
            _logger = logger;
            _commandHandler = commandHandler;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> ReportIncidentAsync(Guid id, ReportIncidentCommand command)
        {
            try
            {
                command = command with { Id = id };
                await _commandHandler.HandleAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Report incident request completed successfully!"
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
                _logger.Log(LogLevel.Warning, ex, "Could not retrieve aggregate!");
                return BadRequest(new BaseResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to report incident!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
