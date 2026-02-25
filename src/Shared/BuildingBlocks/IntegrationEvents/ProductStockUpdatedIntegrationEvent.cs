using EventBus.Abstractions;
using System.Text.Json.Serialization;

namespace IntegrationEvents;

/// <summary>
/// Event raised when the stock level of a product is updated.
/// </summary>
public record ProductStockUpdatedIntegrationEvent : IntegrationEvent
{
    private const string REASON_ORDER_CREATED = "OrderCreated";
    private const string REASON_MANUAL_ADJUSTMENT = "ManualAdjustment";

    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// Gets the new stock quantity available.
    /// </summary>
    public int NewStock { get; init; }

    /// <summary>
    /// Gets the reason for the stock update (e.g., "OrderCreated", "ManualAdjustment").
    /// </summary>
    public string Reason { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductStockUpdatedIntegrationEvent"/> class.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="newStock">The new stock quantity available.</param>
    /// <param name="reason">The reason for the stock update.</param>
    [JsonConstructor]
    public ProductStockUpdatedIntegrationEvent(int productId, int newStock, string reason)
    {
        ProductId = productId;
        NewStock = newStock;
        Reason = reason;
    }
}
