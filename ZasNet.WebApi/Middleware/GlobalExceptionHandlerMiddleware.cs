using System.Net;
using System.Text.Json;
using FluentValidation;

namespace ZasNet.WebApi.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Message = exception.Message,
            StatusCode = GetStatusCode(exception)
        };

        // Добавляем детали в зависимости от типа исключения
        if (exception is ValidationException validationException)
        {
            errorResponse.Details = string.Join("; ", validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        }
        else if (exception is InvalidOperationException || exception is ArgumentException)
        {
            errorResponse.Details = string.Empty;
        }
        else
        {
            errorResponse.Details = exception.GetType().Name;
        }

        // В режиме разработки добавляем StackTrace
        if (_environment.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
        }

        context.Response.StatusCode = errorResponse.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest, // 400
            ArgumentNullException => (int)HttpStatusCode.BadRequest, // 400
            ArgumentException => (int)HttpStatusCode.BadRequest, // 400
            InvalidOperationException => (int)HttpStatusCode.BadRequest, // 400
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
            KeyNotFoundException => (int)HttpStatusCode.NotFound, // 404
            _ => (int)HttpStatusCode.InternalServerError // 500
        };
    }
}

