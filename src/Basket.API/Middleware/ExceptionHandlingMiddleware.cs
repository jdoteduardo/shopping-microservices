using System.Net;
using System.Text.Json;
using Basket.API.Exceptions;
using StackExchange.Redis;

namespace Basket.API.Middleware;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            // Custom domain exceptions
            case BasketNotFoundException basketNotFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Code = basketNotFoundEx.Code;
                response.Message = basketNotFoundEx.Message;
                response.Details = new { userId = basketNotFoundEx.UserId };
                break;

            case BasketItemNotFoundException itemNotFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Code = itemNotFoundEx.Code;
                response.Message = itemNotFoundEx.Message;
                response.Details = new
                {
                    userId = itemNotFoundEx.UserId,
                    productId = itemNotFoundEx.ProductId
                };
                break;

            case InvalidQuantityException quantityEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = quantityEx.Code;
                response.Message = quantityEx.Message;
                response.Details = new { quantity = quantityEx.Quantity };
                break;

            case InvalidPriceException priceEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = priceEx.Code;
                response.Message = priceEx.Message;
                response.Details = new { price = priceEx.Price };
                break;

            case CacheOperationException cacheOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = cacheOpEx.Code;
                response.Message = "A cache error occurred. Please try again later.";
                _logger.LogError(cacheOpEx, "Cache operation '{Operation}' failed", cacheOpEx.Operation);
                break;

            // Redis exceptions
            case RedisConnectionException:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Code = "CACHE_UNAVAILABLE";
                response.Message = "Cache service is temporarily unavailable. Please try again later.";
                break;

            case RedisTimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
                response.Code = "CACHE_TIMEOUT";
                response.Message = "Cache service request timed out. Please try again later.";
                break;

            case RedisException redisEx:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Code = "CACHE_ERROR";
                response.Message = "A cache error occurred. Please try again later.";
                _logger.LogError(redisEx, "Redis error occurred");
                break;

            // Standard exceptions
            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = "INVALID_ARGUMENT";
                response.Message = argEx.Message;
                break;

            case InvalidOperationException invOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = "INVALID_OPERATION";
                response.Message = invOpEx.Message;
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Code = "NOT_FOUND";
                response.Message = exception.Message;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Code = "UNAUTHORIZED";
                response.Message = "You are not authorized to perform this action.";
                break;

            case JsonException jsonEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = "INVALID_JSON";
                response.Message = "Invalid JSON format. Please check your request body.";
                _logger.LogWarning(jsonEx, "Invalid JSON in request");
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = "INTERNAL_ERROR";
                response.Message = "An internal server error occurred. Please try again later.";
                break;
        }

        await WriteResponseAsync(context, response);
    }

    private static async Task WriteResponseAsync(HttpContext context, ErrorResponse response)
    {
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; }
}
