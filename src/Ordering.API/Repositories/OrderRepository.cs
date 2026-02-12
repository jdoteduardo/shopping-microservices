using MongoDB.Driver;
using Ordering.API.Data;
using Ordering.API.Models;

namespace Ordering.API.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(MongoDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        _logger.LogInformation("Getting all orders");
        return await _context.Orders
            .Find(_ => true)
            .SortByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Getting order by id {OrderId}", id);
        return await _context.Orders
            .Find(o => o.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        _logger.LogInformation("Getting order by order number {OrderNumber}", orderNumber);
        return await _context.Orders
            .Find(o => o.OrderNumber == orderNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
    {
        _logger.LogInformation("Getting orders for user {UserId}", userId);
        return await _context.Orders
            .Find(o => o.UserId == userId)
            .SortByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _logger.LogInformation("Creating order {OrderNumber} for user {UserId}", 
            order.OrderNumber, order.UserId);
        
        await _context.Orders.InsertOneAsync(order);
        
        _logger.LogInformation("Order {OrderNumber} created with id {OrderId}", 
            order.OrderNumber, order.Id);
        
        return order;
    }

    public async Task<Order?> UpdateAsync(Order order)
    {
        _logger.LogInformation("Updating order {OrderId}", order.Id);
        
        var result = await _context.Orders.ReplaceOneAsync(
            o => o.Id == order.Id,
            order);

        if (result.ModifiedCount == 0)
        {
            _logger.LogWarning("Order {OrderId} not found for update", order.Id);
            return null;
        }

        _logger.LogInformation("Order {OrderId} updated successfully", order.Id);
        return order;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting order {OrderId}", id);
        
        var result = await _context.Orders.DeleteOneAsync(o => o.Id == id);
        
        if (result.DeletedCount == 0)
        {
            _logger.LogWarning("Order {OrderId} not found for deletion", id);
            return false;
        }

        _logger.LogInformation("Order {OrderId} deleted successfully", id);
        return true;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _context.Orders.CountDocumentsAsync(o => o.Id == id);
        return count > 0;
    }
}
