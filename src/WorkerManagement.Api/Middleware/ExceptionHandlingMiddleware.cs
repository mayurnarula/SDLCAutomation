using Microsoft.AspNetCore.Mvc;
using WorkerManagement.BusinessLogic.Exceptions;

namespace WorkerManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (WorkerValidationException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                new ValidationProblemDetails(new Dictionary<string, string[]>(exception.Errors)));
        }
        catch (DuplicateWorkerException)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "A worker with the supplied identifier already exists.",
                new ProblemDetails
                {
                    Title = "Worker already exists.",
                    Detail = "The request could not be completed because the worker identifier is already in use."
                });
        }
        catch (WorkerNotFoundException)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                "Worker not found.",
                new ProblemDetails
                {
                    Title = "Worker not found.",
                    Detail = "No worker matches the supplied identifier."
                });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                new ProblemDetails
                {
                    Title = "An unexpected error occurred.",
                    Detail = "The request could not be completed."
                });
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        ProblemDetails problem)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        problem.Status = statusCode;
        problem.Title = title;
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
