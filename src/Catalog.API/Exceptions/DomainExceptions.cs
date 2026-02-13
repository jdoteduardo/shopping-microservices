namespace Catalog.API.Exceptions;

/// <summary>
/// Base exception for all domain exceptions
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    protected DomainException(string code, string message, Exception innerException) 
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when a duplicate resource is detected
/// </summary>
public class DuplicateResourceException : DomainException
{
    public string ResourceType { get; }
    public string PropertyName { get; }
    public object? PropertyValue { get; }

    public DuplicateResourceException(string resourceType, string propertyName, object? propertyValue)
        : base("DUPLICATE_RESOURCE", $"A {resourceType} with {propertyName} '{propertyValue}' already exists.")
    {
        ResourceType = resourceType;
        PropertyName = propertyName;
        PropertyValue = propertyValue;
    }
}

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class ResourceNotFoundException : DomainException
{
    public string ResourceType { get; }
    public object? ResourceId { get; }

    public ResourceNotFoundException(string resourceType, object? resourceId)
        : base("RESOURCE_NOT_FOUND", $"{resourceType} with id '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception thrown when a foreign key constraint is violated
/// </summary>
public class ForeignKeyViolationException : DomainException
{
    public string ResourceType { get; }
    public string ReferencedResourceType { get; }
    public object? ReferencedResourceId { get; }

    public ForeignKeyViolationException(string resourceType, string referencedResourceType, object? referencedResourceId)
        : base("FOREIGN_KEY_VIOLATION", 
            $"Cannot perform operation on {resourceType}. Referenced {referencedResourceType} with id '{referencedResourceId}' does not exist or is invalid.")
    {
        ResourceType = resourceType;
        ReferencedResourceType = referencedResourceType;
        ReferencedResourceId = referencedResourceId;
    }
}

/// <summary>
/// Exception thrown when a resource has dependencies and cannot be deleted
/// </summary>
public class ResourceHasDependenciesException : DomainException
{
    public string ResourceType { get; }
    public object? ResourceId { get; }
    public string DependentResourceType { get; }

    public ResourceHasDependenciesException(string resourceType, object? resourceId, string dependentResourceType)
        : base("RESOURCE_HAS_DEPENDENCIES", 
            $"Cannot delete {resourceType} with id '{resourceId}' because it has related {dependentResourceType}.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
        DependentResourceType = dependentResourceType;
    }
}

/// <summary>
/// Exception thrown when a database operation fails
/// </summary>
public class DatabaseOperationException : DomainException
{
    public string Operation { get; }

    public DatabaseOperationException(string operation, string message, Exception? innerException = null)
        : base("DATABASE_ERROR", message, innerException!)
    {
        Operation = operation;
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict is detected
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    public string ResourceType { get; }
    public object? ResourceId { get; }

    public ConcurrencyConflictException(string resourceType, object? resourceId)
        : base("CONCURRENCY_CONFLICT", 
            $"The {resourceType} with id '{resourceId}' was modified by another user. Please refresh and try again.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
