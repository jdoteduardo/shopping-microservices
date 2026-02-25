using EventBus.Abstractions;
using System.Text.Json.Serialization;

namespace IntegrationEvents;

/// <summary>
/// Event raised when the status of an order changes.
/// </summary>
public record OrderStatusChangedIntegrationEvent : IntegrationEvent
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
    /// Gets the unique identifier of the user who owns the order.
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// Gets the previous status of the order.
    /// </summary>
    public string OldStatus { get; init; }

    /// <summary>
    /// Gets the new status of the order.
    /// </summary>
    public string NewStatus { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderStatusChangedIntegrationEvent"/> class.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="orderNumber">The user-friendly order number.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="oldStatus">The previous status of the order.</param>
    /// <param name="newStatus">The new status of the order.</param>
    [JsonConstructor]
    public OrderStatusChangedIntegrationEvent(
        string orderId,
        string orderNumber,
        string userId,
        string oldStatus,
        string newStatus)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        UserId = userId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
