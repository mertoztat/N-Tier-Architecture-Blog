using BlogAPI.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Middlewares;

/// <summary>
/// Turns <see cref="AppException"/>s thrown by services into ProblemDetails responses
/// (404, 403, 400...). Other exceptions are left to the default handling (500).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException appException)
        {
            return false;
        }

        httpContext.Response.StatusCode = appException.StatusCode;
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = appException.StatusCode,
                Detail = appException.Message
            }
        });
    }
}
