using Ordering.API.DTOs;
using Ordering.API.Services;
using Microsoft.AspNetCore.Mvc;
using UserContext;

namespace Ordering.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IUserContext _userContext;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, IUserContext userContext, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders (Admin only)
    /// </summary>
    /// <returns>List of all orders</returns>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        if (!_userContext.IsInRole("Admin"))
        {
            _logger.LogWarning("User {UserId} tried to access all orders without Admin role", _userContext.UserId);
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        _logger.LogInformation("Admin {UserId} getting all orders", _userContext.UserId);
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Get orders for the currently authenticated user
    /// </summary>
    /// <returns>List of current user's orders</returns>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized();
        }

        _logger.LogInformation("User {UserId} getting their orders", _userContext.UserId);
        var orders = await _orderService.GetOrdersByUserIdAsync(_userContext.UserId!);
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto createOrderDto)
    {
        if (!_userContext.IsAuthenticated)
        {
            _logger.LogWarning("Unauthorized attempt to create order");
            return Unauthorized();
        }

        _logger.LogInformation("Creating new order for user {UserId}", _userContext.UserId);

        var order = await _orderService.CreateOrderAsync(createOrderDto, _userContext.UserId);

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
