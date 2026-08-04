using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MotorsShop.Exceptions;

namespace MotorsShop.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        // Log everything; log unexpected errors more loudly
        if (status >= 500)
            _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
        else
            _logger.LogWarning("Handled {Type} on {Path}: {Message}",
                exception.GetType().Name, httpContext.Request.Path, exception.Message);

        ProblemDetails problem;

        if (exception is ValidationException vex && vex.Errors.Count > 0) {
            problem = new ValidationProblemDetails(vex.Errors)
            {
                Title = title,
                Status = status,
                Detail = vex.Message,
                Instance = httpContext.Request.Path
            };
        }
        else {
            problem = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = status >= 500 && !_env.IsDevelopment()
                    ? "An unexpected error occurred."
                    : exception.Message,
                Instance = httpContext.Request.Path
            };
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}