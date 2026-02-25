using EventBus.Abstractions;
using System.Text.Json.Serialization;

namespace IntegrationEvents;

/// <summary>
/// Event raised when a new order is successfully created.
/// </summary>
public record OrderCreatedIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the unique identifier of the order.
    /// </summary>
    public string OrderId { get; init; }

    /// <summary>
    /// Gets the user-friendly order number.
    /// </summary>
    public string OrderNumber { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user who placed the order.
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// Gets the list of items in the order.
    /// </summary>
    public IEnumerable<OrderItemData> Items { get; init; }

    /// <summary>
    /// Gets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderCreatedIntegrationEvent"/> class.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="orderNumber">The user-friendly order number.</param>
    /// <param name="userId">The unique identifier of the user who placed the order.</param>
    /// <param name="items">The list of items in the order.</param>
    /// <param name="totalAmount">The total amount of the order.</param>
    [JsonConstructor]
    public OrderCreatedIntegrationEvent(
        string orderId,
        string orderNumber,
        string userId,
        IEnumerable<OrderItemData> items,
        decimal totalAmount)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        UserId = userId;
        Items = items;
        TotalAmount = totalAmount;
    }
}

/// <summary>
/// Represents the data of an item within an order integration event.
/// </summary>
public record OrderItemData
{
    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// Gets the name of the product.
    /// </summary>
    public string ProductName { get; init; }

    /// <summary>
    /// Gets the quantity of the product ordered.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Gets the unit price of the product.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderItemData"/> class.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="productName">The name of the product.</param>
    /// <param name="quantity">The quantity ordered.</param>
    /// <param name="price">The unit price.</param>
    [JsonConstructor]
    public OrderItemData(int productId, string productName, int quantity, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }
}
