using Ordering.API.DTOs;
using Ordering.API.Models;

namespace Ordering.API.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(string id);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<OrderDto?> UpdateOrderStatusAsync(string id, UpdateOrderStatusDto updateStatusDto);
    Task<bool> CancelOrderAsync(string id);
}
