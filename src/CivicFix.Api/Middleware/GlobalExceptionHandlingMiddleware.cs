using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CivicFix.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var statusCode = HttpStatusCode.InternalServerError;
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Title = "An error occurred while processing your request."
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Failure";
                problemDetails.Status = (int)statusCode;
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            case KeyNotFoundException keyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Status = (int)statusCode;
                problemDetails.Detail = keyNotFoundException.Message;
                break;

            case UnauthorizedAccessException unauthorizedException:
                statusCode = HttpStatusCode.Forbidden;
                problemDetails.Title = "Access Denied";
                problemDetails.Status = (int)statusCode;
                problemDetails.Detail = unauthorizedException.Message;
                break;

            case InvalidOperationException invalidOpException:
                statusCode = HttpStatusCode.Conflict;
                problemDetails.Title = "Conflict / Invalid Operation";
                problemDetails.Status = (int)statusCode;
                problemDetails.Detail = invalidOpException.Message;
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Status = (int)statusCode;
                problemDetails.Detail = "An unexpected server error occurred. Please contact support.";
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
