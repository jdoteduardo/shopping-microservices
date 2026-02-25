using EventBus.Abstractions;
using IntegrationEvents;
using Catalog.API.Repositories;
using Catalog.API.Data;
using Catalog.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.IntegrationEventHandlers;

/// <summary>
/// Handles OrderCreatedIntegrationEvent by decrementing the stock 
/// of each product included in the newly created order.
/// </summary>
public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly IProductRepository _productRepository;
    private readonly CatalogContext _catalogContext;
    private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;

    public OrderCreatedIntegrationEventHandler(
        IProductRepository productRepository,
        CatalogContext catalogContext,
        ILogger<OrderCreatedIntegrationEventHandler> logger)
    {
        _productRepository = productRepository;
        _catalogContext = catalogContext;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedIntegrationEvent @event)
    {
        _logger.LogInformation(
            "Handling OrderCreatedIntegrationEvent: Id={EventId}, OrderNumber={OrderNumber}, UserId={UserId}",
            @event.Id, @event.OrderNumber, @event.UserId);

        // Idempotency check
        if (await _catalogContext.ProcessedEvents.AnyAsync(e => e.Id == @event.Id))
        {
            _logger.LogWarning("Event {EventId} was already processed. Skipping.", @event.Id);
            return;
        }

        using var transaction = await _catalogContext.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in @event.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);

                if (product == null)
                {
                    _logger.LogWarning(
                        "Product {ProductId} ({ProductName}) not found while processing order {OrderNumber}. Skipping stock decrement.",
                        item.ProductId, item.ProductName, @event.OrderNumber);
                    continue;
                }

                if (product.Stock < item.Quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for product {ProductId} ({ProductName}). " +
                        "Current stock: {CurrentStock}, Requested: {Requested}. Order: {OrderNumber}",
                        item.ProductId, item.ProductName, product.Stock, item.Quantity, @event.OrderNumber);
                    
                    product.Stock = 0;
                }
                else
                {
                    product.Stock -= item.Quantity;
                }

                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product);

                _logger.LogInformation(
                    "Decremented stock for product {ProductId} ({ProductName}) by {Quantity}. New stock: {NewStock}. Order: {OrderNumber}",
                    item.ProductId, item.ProductName, item.Quantity, product.Stock, @event.OrderNumber);
            }

            // Record event as processed
            _catalogContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = @event.Id,
                EventType = nameof(OrderCreatedIntegrationEvent),
                ProcessedAt = DateTime.UtcNow
            });

            await _catalogContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Successfully processed OrderCreatedIntegrationEvent {EventId} for order {OrderNumber}", 
                @event.Id, @event.OrderNumber);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing OrderCreatedIntegrationEvent {EventId} for order {OrderNumber}", 
                @event.Id, @event.OrderNumber);
            throw; // Rethrow to allow EventBus retry if configured
        }
    }
}

