using EventBus.Abstractions;
using System.Text.Json.Serialization;

namespace IntegrationEvents;

/// <summary>
/// Event raised when the price of a product changes.
/// </summary>
public record ProductPriceChangedIntegrationEvent : IntegrationEvent
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
    /// Gets the previous price of the product.
    /// </summary>
    public decimal OldPrice { get; init; }

    /// <summary>
    /// Gets the new price of the product.
    /// </summary>
    public decimal NewPrice { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductPriceChangedIntegrationEvent"/> class.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="productName">The name of the product.</param>
    /// <param name="oldPrice">The previous price of the product.</param>
    /// <param name="newPrice">The new price of the product.</param>
    [JsonConstructor]
    public ProductPriceChangedIntegrationEvent(int productId, string productName, decimal oldPrice, decimal newPrice)
    {
        ProductId = productId;
        ProductName = productName;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}
