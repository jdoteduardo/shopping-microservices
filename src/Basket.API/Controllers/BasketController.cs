using Basket.API.DTOs;
using Basket.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketService basketService, ILogger<BasketController> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    /// <summary>
    /// Get basket by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User's basket</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BasketDto>> GetBasket(string userId)
    {
        _logger.LogInformation("Getting basket for user {UserId}", userId);
        
        var basket = await _basketService.GetBasketAsync(userId);
        return Ok(basket);
    }

    /// <summary>
    /// Create or update basket
    /// </summary>
    /// <param name="updateBasketDto">Basket data</param>
    /// <returns>Updated basket</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BasketDto>> UpdateBasket([FromBody] UpdateBasketDto updateBasketDto)
    {
        _logger.LogInformation("Updating basket for user {UserId}", updateBasketDto.UserId);

        var basket = await _basketService.UpdateBasketAsync(updateBasketDto);

        if (basket == null)
        {
            _logger.LogWarning("Failed to update basket for user {UserId}", updateBasketDto.UserId);
            return BadRequest(new { message = "Failed to update basket" });
        }

        return Ok(basket);
    }

    /// <summary>
    /// Delete basket
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBasket(string userId)
    {
        _logger.LogInformation("Deleting basket for user {UserId}", userId);

        var deleted = await _basketService.DeleteBasketAsync(userId);

        if (!deleted)
        {
            _logger.LogWarning("Basket not found for user {UserId}", userId);
            return NotFound(new { message = $"Basket for user {userId} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Add item to basket
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="addItemDto">Item data</param>
    /// <returns>Updated basket</returns>
    [HttpPost("{userId}/items")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BasketDto>> AddItem(string userId, [FromBody] AddItemDto addItemDto)
    {
        _logger.LogInformation("Adding item {ProductId} to basket for user {UserId}", 
            addItemDto.ProductId, userId);

        var basket = await _basketService.AddItemToBasketAsync(userId, addItemDto);

        if (basket == null)
        {
            _logger.LogWarning("Failed to add item to basket for user {UserId}", userId);
            return BadRequest(new { message = "Failed to add item to basket" });
        }

        return Ok(basket);
    }

    /// <summary>
    /// Update item quantity
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Quantity update request</param>
    /// <returns>Updated basket</returns>
    [HttpPut("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BasketDto>> UpdateItemQuantity(
        string userId, 
        int productId, 
        [FromBody] UpdateQuantityRequest request)
    {
        _logger.LogInformation("Updating quantity for item {ProductId} in basket for user {UserId}", 
            productId, userId);

        if (request.Quantity < 0)
        {
            return BadRequest(new { message = "Quantity cannot be negative" });
        }

        var basket = await _basketService.UpdateItemQuantityAsync(userId, productId, request.Quantity);

        if (basket == null)
        {
            _logger.LogWarning("Item {ProductId} not found in basket for user {UserId}", productId, userId);
            return NotFound(new { message = $"Item {productId} not found in basket for user {userId}" });
        }

        return Ok(basket);
    }

    /// <summary>
    /// Remove item from basket
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="productId">Product ID</param>
    /// <returns>Updated basket</returns>
    [HttpDelete("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BasketDto>> RemoveItem(string userId, int productId)
    {
        _logger.LogInformation("Removing item {ProductId} from basket for user {UserId}", productId, userId);

        var basket = await _basketService.RemoveItemFromBasketAsync(userId, productId);

        if (basket == null)
        {
            _logger.LogWarning("Item {ProductId} not found in basket for user {UserId}", productId, userId);
            return NotFound(new { message = $"Item {productId} not found in basket for user {userId}" });
        }

        return Ok(basket);
    }
}

/// <summary>
/// Request model for updating item quantity
/// </summary>
public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}
