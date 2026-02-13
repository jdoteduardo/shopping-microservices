namespace Ordering.API.Exceptions;

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
/// Exception thrown when an order is not found
/// </summary>
public class OrderNotFoundException : DomainException
{
    public string OrderId { get; }

    public OrderNotFoundException(string orderId)
        : base("ORDER_NOT_FOUND", $"Order with id '{orderId}' was not found.")
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Exception thrown when an invalid order status transition is attempted
/// </summary>
public class InvalidOrderStatusTransitionException : DomainException
{
    public string CurrentStatus { get; }
    public string TargetStatus { get; }

    public InvalidOrderStatusTransitionException(string currentStatus, string targetStatus)
        : base("INVALID_STATUS_TRANSITION", 
            $"Cannot change order status from '{currentStatus}' to '{targetStatus}'. This transition is not allowed.")
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }
}

/// <summary>
/// Exception thrown when an order cannot be cancelled
/// </summary>
public class OrderCannotBeCancelledException : DomainException
{
    public string OrderId { get; }
    public string CurrentStatus { get; }

    public OrderCannotBeCancelledException(string orderId, string currentStatus)
        : base("ORDER_CANNOT_BE_CANCELLED", 
            $"Order '{orderId}' cannot be cancelled. Orders can only be cancelled when in 'Pending' or 'Confirmed' status. Current status: '{currentStatus}'.")
    {
        OrderId = orderId;
        CurrentStatus = currentStatus;
    }
}

/// <summary>
/// Exception thrown when order has no items
/// </summary>
public class EmptyOrderException : DomainException
{
    public EmptyOrderException()
        : base("EMPTY_ORDER", "Order must contain at least one item.")
    {
    }
}

/// <summary>
/// Exception thrown when user id is missing
/// </summary>
public class InvalidUserIdException : DomainException
{
    public InvalidUserIdException()
        : base("INVALID_USER_ID", "User ID is required and cannot be empty.")
    {
    }
}

/// <summary>
/// Exception thrown when shipping address is invalid
/// </summary>
public class InvalidShippingAddressException : DomainException
{
    public string MissingField { get; }

    public InvalidShippingAddressException(string missingField)
        : base("INVALID_SHIPPING_ADDRESS", $"Shipping address is invalid. Missing required field: '{missingField}'.")
    {
        MissingField = missingField;
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
/// Exception thrown when order item is invalid
/// </summary>
public class InvalidOrderItemException : DomainException
{
    public int ProductId { get; }
    public string Reason { get; }

    public InvalidOrderItemException(int productId, string reason)
        : base("INVALID_ORDER_ITEM", $"Order item with product id '{productId}' is invalid: {reason}")
    {
        ProductId = productId;
        Reason = reason;
    }
}

/// <summary>
/// Exception thrown when order number already exists
/// </summary>
public class DuplicateOrderNumberException : DomainException
{
    public string OrderNumber { get; }

    public DuplicateOrderNumberException(string orderNumber)
        : base("DUPLICATE_ORDER_NUMBER", $"Order with number '{orderNumber}' already exists.")
    {
        OrderNumber = orderNumber;
    }
}
