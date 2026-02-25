using AutoMapper;
using EventBus.Abstractions;
using IntegrationEvents;
using Ordering.API.DTOs;
using Ordering.API.Models;
using Ordering.API.Repositories;

namespace Ordering.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly IEventBus _eventBus;

    public OrderService(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<OrderService> logger,
        IEventBus eventBus)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        _logger.LogInformation("Service: Getting all orders");
        var orders = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(string id)
    {
        _logger.LogInformation("Service: Getting order by id {OrderId}", id);
        var order = await _repository.GetByIdAsync(id);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId)
    {
        _logger.LogInformation("Service: Getting orders for user {UserId}", userId);
        var orders = await _repository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, string? userId)
    {
        _logger.LogInformation("Service: Creating order for user {UserId}", userId);

        var order = _mapper.Map<Order>(createOrderDto);
        order.UserId = userId ?? string.Empty;
        
        // Generate order number
        order.OrderNumber = GenerateOrderNumber();
        
        // Set order date
        order.OrderDate = DateTime.UtcNow;
        
        // Set initial status
        order.Status = OrderStatus.Pending;
        
        // Calculate total amount
        order.TotalAmount = CalculateTotalAmount(order.Items);

        _logger.LogInformation("Generated order number: {OrderNumber}, Total: {TotalAmount}", 
            order.OrderNumber, order.TotalAmount);

        var createdOrder = await _repository.CreateAsync(order);

        // Publish OrderCreatedIntegrationEvent
        try
        {
            _logger.LogInformation("Publishing OrderCreatedIntegrationEvent for order {OrderNumber}", createdOrder.OrderNumber);

            var orderCreatedEvent = new OrderCreatedIntegrationEvent(
                orderId: createdOrder.Id!,
                orderNumber: createdOrder.OrderNumber,
                userId: createdOrder.UserId,
                items: createdOrder.Items.Select(i => new OrderItemData(
                    productId: i.ProductId,
                    productName: i.ProductName,
                    quantity: i.Quantity,
                    price: i.Price
                )),
                totalAmount: createdOrder.TotalAmount
            );

            await _eventBus.PublishAsync(orderCreatedEvent);

            _logger.LogInformation("Successfully published OrderCreatedIntegrationEvent for order {OrderNumber}", createdOrder.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish OrderCreatedIntegrationEvent for order {OrderNumber}. " +
                "The order was created successfully but the event could not be published.", createdOrder.OrderNumber);
        }

        return _mapper.Map<OrderDto>(createdOrder);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto updateStatusDto)
    {
        _logger.LogInformation("Service: Updating status for order {OrderId} to {Status}", 
            id, updateStatusDto.Status);

        var order = await _repository.GetByIdAsync(id);
        
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return null;
        }

        // Validate status transition
        if (!IsValidStatusTransition(order.Status, updateStatusDto.Status))
        {
            _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for order {OrderId}",
                order.Status, updateStatusDto.Status, id);
            throw new InvalidOperationException(
                $"Cannot change order status from {order.Status} to {updateStatusDto.Status}");
        }

        var oldStatus = order.Status;
        order.Status = updateStatusDto.Status;
        var updatedOrder = await _repository.UpdateAsync(order);

        // Publish OrderStatusChangedIntegrationEvent
        if (updatedOrder != null)
        {
            try
            {
                _logger.LogInformation("Publishing OrderStatusChangedIntegrationEvent for order {OrderNumber}: {OldStatus} -> {NewStatus}",
                    updatedOrder.OrderNumber, oldStatus, updatedOrder.Status);

                var statusChangedEvent = new OrderStatusChangedIntegrationEvent(
                    orderId: updatedOrder.Id!,
                    orderNumber: updatedOrder.OrderNumber,
                    userId: updatedOrder.UserId,
                    oldStatus: oldStatus.ToString(),
                    newStatus: updatedOrder.Status.ToString()
                );

                await _eventBus.PublishAsync(statusChangedEvent);

                _logger.LogInformation("Successfully published OrderStatusChangedIntegrationEvent for order {OrderNumber}",
                    updatedOrder.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish OrderStatusChangedIntegrationEvent for order {OrderNumber}",
                    updatedOrder.OrderNumber);
            }
        }

        return updatedOrder != null ? _mapper.Map<OrderDto>(updatedOrder) : null;
    }

    public async Task<bool> CancelOrderAsync(string id)
    {
        _logger.LogInformation("Service: Cancelling order {OrderId}", id);

        var order = await _repository.GetByIdAsync(id);
        
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return false;
        }

        // Can only cancel pending or confirmed orders
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
        {
            _logger.LogWarning("Cannot cancel order {OrderId} with status {Status}", id, order.Status);
            throw new InvalidOperationException(
                $"Cannot cancel order with status {order.Status}. Only Pending or Confirmed orders can be cancelled.");
        }

        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        var result = await _repository.UpdateAsync(order);

        // Publish status changed event for cancellation
        if (result != null)
        {
            try
            {
                var statusChangedEvent = new OrderStatusChangedIntegrationEvent(
                    orderId: result.Id!,
                    orderNumber: result.OrderNumber,
                    userId: result.UserId,
                    oldStatus: oldStatus.ToString(),
                    newStatus: OrderStatus.Cancelled.ToString()
                );

                await _eventBus.PublishAsync(statusChangedEvent);
                _logger.LogInformation("Published OrderStatusChangedIntegrationEvent (cancellation) for order {OrderNumber}", result.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish cancellation event for order {OrderId}", id);
            }
        }

        return result != null;
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }

    private static decimal CalculateTotalAmount(List<OrderItem> items)
    {
        return items.Sum(item => item.Price * item.Quantity);
    }

    private static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Define valid transitions
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            _ => false
        };
    }
}

