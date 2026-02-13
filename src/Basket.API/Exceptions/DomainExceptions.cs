namespace Basket.API.Exceptions;

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
/// Exception thrown when a basket is not found
/// </summary>
public class BasketNotFoundException : DomainException
{
    public string UserId { get; }

    public BasketNotFoundException(string userId)
        : base("BASKET_NOT_FOUND", $"Basket for user '{userId}' was not found.")
    {
        UserId = userId;
    }
}

/// <summary>
/// Exception thrown when an item is not found in the basket
/// </summary>
public class BasketItemNotFoundException : DomainException
{
    public string UserId { get; }
    public int ProductId { get; }

    public BasketItemNotFoundException(string userId, int productId)
        : base("BASKET_ITEM_NOT_FOUND", $"Item with product id '{productId}' was not found in the basket for user '{userId}'.")
    {
        UserId = userId;
        ProductId = productId;
    }
}

/// <summary>
/// Exception thrown when a cache operation fails
/// </summary>
public class CacheOperationException : DomainException
{
    public string Operation { get; }

    public CacheOperationException(string operation, string message, Exception? innerException = null)
        : base("CACHE_ERROR", message, innerException!)
    {
        Operation = operation;
    }
}

/// <summary>
/// Exception thrown when item quantity is invalid
/// </summary>
public class InvalidQuantityException : DomainException
{
    public int Quantity { get; }

    public InvalidQuantityException(int quantity)
        : base("INVALID_QUANTITY", $"Quantity '{quantity}' is invalid. Quantity must be a positive number.")
    {
        Quantity = quantity;
    }
}

/// <summary>
/// Exception thrown when price is invalid
/// </summary>
public class InvalidPriceException : DomainException
{
    public decimal Price { get; }

    public InvalidPriceException(decimal price)
        : base("INVALID_PRICE", $"Price '{price}' is invalid. Price must be greater than zero.")
    {
        Price = price;
    }
}
