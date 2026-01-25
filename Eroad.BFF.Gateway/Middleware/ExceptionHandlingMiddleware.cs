using Grpc.Core;
using System.Net;
using System.Text.Json;

namespace Eroad.BFF.Gateway.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
        {
            _logger.LogWarning("gRPC InvalidArgument error: {Detail}", ex.Status.Detail);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Status.Detail);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            _logger.LogWarning("gRPC NotFound error: {Detail}", ex.Status.Detail);
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Status.Detail);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
        {
            _logger.LogWarning("gRPC AlreadyExists error: {Detail}", ex.Status.Detail);
            await HandleExceptionAsync(context, HttpStatusCode.Conflict, ex.Status.Detail);
        }
        catch (RpcException ex)
        {
            _logger.LogError("gRPC error: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An error occurred while processing your request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An error occurred while processing your request");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Message = message
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
