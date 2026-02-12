using Ordering.API.DTOs;
using Ordering.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ordering.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders (Admin only - prepare for JWT)
    /// </summary>
    /// <returns>List of all orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        _logger.LogInformation("Getting all orders");
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(string id)
    {
        _logger.LogInformation("Getting order with id {OrderId}", id);
        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Order with id {OrderId} not found", id);
            return NotFound(new { message = $"Order with id {id} not found" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Get orders by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's orders</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByUserId(string userId)
    {
        _logger.LogInformation("Getting orders for user {UserId}", userId);
        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return Ok(orders);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="createOrderDto">Order data</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto createOrderDto)
    {
        _logger.LogInformation("Creating new order for user {UserId}", createOrderDto.UserId);

        var order = await _orderService.CreateOrderAsync(createOrderDto);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="updateStatusDto">New status</param>
    /// <returns>Updated order</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(string id, [FromBody] UpdateOrderStatusDto updateStatusDto)
    {
        _logger.LogInformation("Updating status for order {OrderId} to {Status}", id, updateStatusDto.Status);

        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, updateStatusDto);

            if (order == null)
            {
                _logger.LogWarning("Order with id {OrderId} not found", id);
                return NotFound(new { message = $"Order with id {id} not found" });
            }

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid status transition for order {OrderId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(string id)
    {
        _logger.LogInformation("Cancelling order {OrderId}", id);

        try
        {
            var result = await _orderService.CancelOrderAsync(id);

            if (!result)
            {
                _logger.LogWarning("Order with id {OrderId} not found", id);
                return NotFound(new { message = $"Order with id {id} not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot cancel order {OrderId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}
