using Breeders.Api.Models.DTOs;
using Breeders.Api.Exceptions;

namespace Breeders.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Domain error occurred: {Message}",
                exception.Message);

            await WriteErrorResponseAsync(
                context,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unexpected error occurred.");

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "internal_server_error",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Message = message,
            TraceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}