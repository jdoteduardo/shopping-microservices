using System.Net;
using System.Text.Json;
using Catalog.API.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Middleware;

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
            case DuplicateResourceException duplicateEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = duplicateEx.Code;
                response.Message = duplicateEx.Message;
                response.Details = new
                {
                    resourceType = duplicateEx.ResourceType,
                    propertyName = duplicateEx.PropertyName,
                    propertyValue = duplicateEx.PropertyValue
                };
                break;

            case ResourceNotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Code = notFoundEx.Code;
                response.Message = notFoundEx.Message;
                response.Details = new
                {
                    resourceType = notFoundEx.ResourceType,
                    resourceId = notFoundEx.ResourceId
                };
                break;

            case ForeignKeyViolationException fkEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = fkEx.Code;
                response.Message = fkEx.Message;
                response.Details = new
                {
                    resourceType = fkEx.ResourceType,
                    referencedResourceType = fkEx.ReferencedResourceType,
                    referencedResourceId = fkEx.ReferencedResourceId
                };
                break;

            case ResourceHasDependenciesException depEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = depEx.Code;
                response.Message = depEx.Message;
                response.Details = new
                {
                    resourceType = depEx.ResourceType,
                    resourceId = depEx.ResourceId,
                    dependentResourceType = depEx.DependentResourceType
                };
                break;

            case ConcurrencyConflictException concurrencyEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = concurrencyEx.Code;
                response.Message = concurrencyEx.Message;
                break;

            case DatabaseOperationException dbOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = dbOpEx.Code;
                response.Message = "A database error occurred. Please try again later.";
                _logger.LogError(dbOpEx, "Database operation '{Operation}' failed", dbOpEx.Operation);
                break;

            // Entity Framework exceptions - order matters! DbUpdateConcurrencyException inherits from DbUpdateException
            case DbUpdateConcurrencyException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Code = "CONCURRENCY_CONFLICT";
                response.Message = "The resource was modified by another user. Please refresh and try again.";
                break;

            case DbUpdateException dbUpdateEx:
                await HandleDbUpdateExceptionAsync(context, dbUpdateEx, response);
                return;

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

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = "INTERNAL_ERROR";
                response.Message = "An internal server error occurred. Please try again later.";
                break;
        }

        await WriteResponseAsync(context, response);
    }

    private async Task HandleDbUpdateExceptionAsync(HttpContext context, DbUpdateException exception, ErrorResponse response)
    {
        // Check for SQL Server specific errors
        if (exception.InnerException is SqlException sqlEx)
        {
            switch (sqlEx.Number)
            {
                // Unique constraint violation
                case 2601: // Cannot insert duplicate key row
                case 2627: // Violation of UNIQUE KEY constraint
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Code = "DUPLICATE_RESOURCE";
                    response.Message = ExtractDuplicateKeyMessage(sqlEx.Message);
                    break;

                // Foreign key violation
                case 547: // FK constraint violation
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Code = "FOREIGN_KEY_VIOLATION";
                    response.Message = ExtractForeignKeyMessage(sqlEx.Message);
                    break;

                // Cannot insert null
                case 515:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Code = "NULL_VALUE_NOT_ALLOWED";
                    response.Message = "A required field is missing. Please check your input.";
                    break;

                // String or binary data would be truncated
                case 8152:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Code = "DATA_TOO_LONG";
                    response.Message = "One or more field values exceed the maximum allowed length.";
                    break;

                // Arithmetic overflow
                case 8115:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Code = "VALUE_OUT_OF_RANGE";
                    response.Message = "One or more numeric values are out of the allowed range.";
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Code = "DATABASE_ERROR";
                    response.Message = "A database error occurred. Please try again later.";
                    _logger.LogError(exception, "Unhandled SQL error {ErrorNumber}: {Message}", sqlEx.Number, sqlEx.Message);
                    break;
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Code = "DATABASE_ERROR";
            response.Message = "A database error occurred. Please try again later.";
        }

        await WriteResponseAsync(context, response);
    }

    private static string ExtractDuplicateKeyMessage(string sqlMessage)
    {
        // Try to extract meaningful info from SQL error message
        // Example: "Cannot insert duplicate key row in object 'dbo.Categories' with unique index 'IX_Categories_Name'. The duplicate key value is (Electronics)."
        
        try
        {
            // Extract table name
            var tableStart = sqlMessage.IndexOf("object '") + 8;
            var tableEnd = sqlMessage.IndexOf("'", tableStart);
            var tableName = tableStart > 7 && tableEnd > tableStart 
                ? sqlMessage[tableStart..tableEnd].Replace("dbo.", "") 
                : "resource";

            // Extract duplicate value
            var valueStart = sqlMessage.IndexOf("duplicate key value is (") + 24;
            var valueEnd = sqlMessage.IndexOf(")", valueStart);
            var value = valueStart > 23 && valueEnd > valueStart 
                ? sqlMessage[valueStart..valueEnd] 
                : "unknown";

            // Extract index/column name
            var indexStart = sqlMessage.IndexOf("index '") + 7;
            var indexEnd = sqlMessage.IndexOf("'", indexStart);
            var indexName = indexStart > 6 && indexEnd > indexStart 
                ? sqlMessage[indexStart..indexEnd] 
                : "";

            // Try to extract column name from index name (e.g., IX_Categories_Name -> Name)
            var columnName = "value";
            if (!string.IsNullOrEmpty(indexName))
            {
                var parts = indexName.Split('_');
                if (parts.Length >= 3)
                {
                    columnName = parts[^1]; // Last part is usually the column name
                }
            }

            return $"A {tableName.TrimEnd('s')} with this {columnName.ToLower()} '{value}' already exists.";
        }
        catch
        {
            return "A resource with this value already exists. Please use a different value.";
        }
    }

    private static string ExtractForeignKeyMessage(string sqlMessage)
    {
        // Example: "The INSERT statement conflicted with the FOREIGN KEY constraint..."
        try
        {
            if (sqlMessage.Contains("FOREIGN KEY constraint"))
            {
                if (sqlMessage.Contains("INSERT"))
                {
                    return "The referenced resource does not exist. Please check your input.";
                }
                if (sqlMessage.Contains("DELETE"))
                {
                    return "Cannot delete this resource because it is referenced by other resources.";
                }
                if (sqlMessage.Contains("UPDATE"))
                {
                    return "Cannot update this resource because the referenced resource does not exist.";
                }
            }
            return "A referential integrity constraint was violated. Please check your input.";
        }
        catch
        {
            return "A referential integrity constraint was violated. Please check your input.";
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
