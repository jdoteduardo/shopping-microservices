using System.Net;
using System.Text.Json;
using MongoDB.Driver;
using Ordering.API.Exceptions;

namespace Ordering.API.Middleware;

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
            case OrderNotFoundException orderNotFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Code = orderNotFoundEx.Code;
                response.Message = orderNotFoundEx.Message;
                response.Details = new { orderId = orderNotFoundEx.OrderId };
                break;

            case InvalidOrderStatusTransitionException statusTransitionEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = statusTransitionEx.Code;
                response.Message = statusTransitionEx.Message;
                response.Details = new
                {
                    currentStatus = statusTransitionEx.CurrentStatus,
                    targetStatus = statusTransitionEx.TargetStatus
                };
                break;

            case OrderCannotBeCancelledException cancelEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = cancelEx.Code;
                response.Message = cancelEx.Message;
                response.Details = new
                {
                    orderId = cancelEx.OrderId,
                    currentStatus = cancelEx.CurrentStatus
                };
                break;

            case EmptyOrderException emptyOrderEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = emptyOrderEx.Code;
                response.Message = emptyOrderEx.Message;
                break;

            case InvalidUserIdException userIdEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = userIdEx.Code;
                response.Message = userIdEx.Message;
                break;

            case InvalidShippingAddressException addressEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = addressEx.Code;
                response.Message = addressEx.Message;
                response.Details = new { missingField = addressEx.MissingField };
                break;

            case InvalidOrderItemException itemEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = itemEx.Code;
                response.Message = itemEx.Message;
                response.Details = new
                {
                    productId = itemEx.ProductId,
                    reason = itemEx.Reason
                };
                break;

            case DuplicateOrderNumberException duplicateEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = duplicateEx.Code;
                response.Message = duplicateEx.Message;
                response.Details = new { orderNumber = duplicateEx.OrderNumber };
                break;

            case DatabaseOperationException dbOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = dbOpEx.Code;
                response.Message = "A database error occurred. Please try again later.";
                _logger.LogError(dbOpEx, "Database operation '{Operation}' failed", dbOpEx.Operation);
                break;

            // MongoDB exceptions
            case MongoConnectionException:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Code = "DATABASE_UNAVAILABLE";
                response.Message = "Database service is temporarily unavailable. Please try again later.";
                break;

            case MongoCommandException mongoCommandEx:
                await HandleMongoCommandExceptionAsync(context, mongoCommandEx, response);
                return;

            case MongoWriteException mongoWriteEx:
                await HandleMongoWriteExceptionAsync(context, mongoWriteEx, response);
                return;

            case MongoException mongoEx:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response.Code = "DATABASE_ERROR";
                response.Message = "A database error occurred. Please try again later.";
                _logger.LogError(mongoEx, "MongoDB error occurred");
                break;

            case TimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
                response.Code = "REQUEST_TIMEOUT";
                response.Message = "The request timed out. Please try again later.";
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

            case FormatException formatEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = "INVALID_FORMAT";
                response.Message = "Invalid format. Please check your input.";
                _logger.LogWarning(formatEx, "Invalid format in request");
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = "INTERNAL_ERROR";
                response.Message = "An internal server error occurred. Please try again later.";
                break;
        }

        await WriteResponseAsync(context, response);
    }

    private async Task HandleMongoCommandExceptionAsync(HttpContext context, MongoCommandException exception, ErrorResponse response)
    {
        switch (exception.Code)
        {
            case 11000: // Duplicate key error
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = "DUPLICATE_RESOURCE";
                response.Message = ExtractDuplicateKeyMessage(exception.Message);
                break;

            case 13: // Unauthorized
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Code = "DATABASE_UNAUTHORIZED";
                response.Message = "Database authentication failed.";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = "DATABASE_ERROR";
                response.Message = "A database error occurred. Please try again later.";
                _logger.LogError(exception, "MongoDB command error {Code}: {Message}", exception.Code, exception.Message);
                break;
        }

        await WriteResponseAsync(context, response);
    }

    private async Task HandleMongoWriteExceptionAsync(HttpContext context, MongoWriteException exception, ErrorResponse response)
    {
        var writeError = exception.WriteError;

        switch (writeError.Category)
        {
            case ServerErrorCategory.DuplicateKey:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = "DUPLICATE_RESOURCE";
                response.Message = ExtractDuplicateKeyMessage(writeError.Message);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = "DATABASE_ERROR";
                response.Message = "A database error occurred. Please try again later.";
                _logger.LogError(exception, "MongoDB write error: {Message}", writeError.Message);
                break;
        }

        await WriteResponseAsync(context, response);
    }

    private static string ExtractDuplicateKeyMessage(string mongoMessage)
    {
        try
        {
            // Try to extract meaningful info from MongoDB error message
            // Example: "E11000 duplicate key error collection: OrderingDb.orders index: orderNumber_1 dup key: { orderNumber: \"ORD-123\" }"

            if (mongoMessage.Contains("orderNumber"))
            {
                return "An order with this order number already exists.";
            }

            if (mongoMessage.Contains("index:"))
            {
                var indexStart = mongoMessage.IndexOf("index:") + 6;
                var indexEnd = mongoMessage.IndexOf(" ", indexStart + 1);
                if (indexEnd > indexStart)
                {
                    var indexName = mongoMessage[indexStart..indexEnd].Trim();
                    var fieldName = indexName.Replace("_1", "").Replace("_-1", "");
                    return $"A resource with this {fieldName} already exists.";
                }
            }

            return "A resource with this value already exists. Please use a different value.";
        }
        catch
        {
            return "A resource with this value already exists. Please use a different value.";
        }
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
