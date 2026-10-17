using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.Exceptions;

namespace SkirtPling.Api.Middleware;

/// <summary>
/// Turns exceptions into RFC 7807 problem responses. Business exceptions keep their message;
/// unexpected ones are logged and returned as a generic 500 so internal details never leak.
/// </summary>
public class AppExceptionHandler(IProblemDetailsService problemDetails, ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Request not allowed", exception.Message),
            ServiceUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Service unavailable", exception.Message),
            OperationCanceledException when context.RequestAborted.IsCancellationRequested => (499, "Request cancelled", null),
            _ => (StatusCodes.Status500InternalServerError, "Something went wrong", "An unexpected error occurred. Please try again.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail }
        });
    }
}
