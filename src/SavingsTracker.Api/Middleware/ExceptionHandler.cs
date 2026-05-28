using Microsoft.AspNetCore.Diagnostics;
using SavingsTracker.Api.DTOs;

namespace SavingsTracker.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unexpected error occurred");

        var (responseMsg, responseCode) = exception switch
        {
            UnauthorizedAccessException => ("Unauthorized access", 401),
            KeyNotFoundException => ("Resource not found", 404),
            ArgumentException => (exception.Message, 400),
            _ => ("An unexpected error occurred", 500)
        };

        httpContext.Response.StatusCode = responseCode;

        await httpContext.Response.WriteAsJsonAsync(new ApiResponse<string>(
            ResponseMsg: responseMsg,
            ResponseDetails: null,
            ResponseCode: responseCode
        ), cancellationToken);

        return true;
    }
}