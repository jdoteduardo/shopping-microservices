using Basket.API.DTOs;
using Basket.API.Services;
using Microsoft.AspNetCore.Mvc;
using UserContext;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;
    private readonly IUserContext _userContext;
    private readonly ILogger<BasketController> _logger;

    public BasketController(
        IBasketService basketService, 
        IUserContext userContext,
        ILogger<BasketController> logger)
    {
        _basketService = basketService;
        _userContext = userContext;
        _logger = logger;
    }

    private string? UserId => _userContext.UserId;

    /// <summary>
    /// Get basket for currently authenticated user
    /// </summary>
    /// <returns>User's basket</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BasketDto>> GetBasket()
    {
        if (!_userContext.IsAuthenticated || UserId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Getting basket for user {UserId}", UserId);
        
        var basket = await _basketService.GetBasketAsync(UserId);
        return Ok(basket);
    }

    /// <summary>
    /// Create or update basket for currently authenticated user
    /// </summary>
    /// <param name="updateBasketDto">Basket data</param>
    /// <returns>Updated basket</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BasketDto>> UpdateBasket([FromBody] UpdateBasketDto updateBasketDto)
    {
        if (!_userContext.IsAuthenticated || UserId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Updating basket for user {UserId}", UserId);

        var basket = await _basketService.UpdateBasketAsync(UserId, updateBasketDto);

        if (basket == null)
        {
            _logger.LogWarning("Failed to update basket for user {UserId}", UserId);
            return BadRequest(new { message = "Failed to update basket" });
        }

        return Ok(basket);
    }

    /// <summary>
    /// Clear basket for currently authenticated user
    /// </summary>
    /// <returns>No content</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBasket()
    {
        if (!_userContext.IsAuthenticated || UserId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Deleting basket for user {UserId}", UserId);

        var deleted = await _basketService.DeleteBasketAsync(UserId);

        if (!deleted)
        {
            _logger.LogWarning("Basket not found for user {UserId}", UserId);
            return NotFound(new { message = $"Basket for user {UserId} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Add item to current user's basket
    /// </summary>
    /// <param name="addItemDto">Item data</param>
    /// <returns>Updated basket</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BasketDto>> AddItem([FromBody] AddItemDto addItemDto)
    {
        if (!_userContext.IsAuthenticated || UserId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Adding item {ProductId} to basket for user {UserId}", 
            addItemDto.ProductId, UserId);

        var basket = await _basketService.AddItemToBasketAsync(UserId, addItemDto);

        if (basket == null)
        {
            _logger.LogWarning("Failed to add item to basket for user {UserId}", UserId);
            return BadRequest(new { message = "Failed to add item to basket" });
        }

        return Ok(basket);
    }

    /// <summary>
    /// Update item quantity in current user's basket
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="request">Quantity update request</param>
    /// <returns>Updated basket</returns>
    [HttpPut("items/{productId}")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BasketDto>> UpdateItemQuantity(
        int productId, 
        [FromBody] UpdateQuantityRequest request)
    {
        if (!_userContext.IsAuthenticated || UserId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Updating quantity for item {ProductId} in basket for user {UserId}", 
            productId, UserId);

        if (request.Quantity < 0)
        {
            return BadRequest(new { message = "Quantity cannot be negative" });
        }

        var basket = await _basketService.UpdateItemQuantityAsync(UserId, productId, request.Quantity);

        if (basket == null)
        {
            _logger.LogWarning("Item {ProductId} not found in basket for user {UserId}", productId, UserId);
            return NotFound(new { message = $"Item {productId} not found in basket for user {UserId}" });
        }

        return Ok(basket);
    }

    /// <summary>
    /// Remove item from current user's basket
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Updated basket</returns>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(typeof(BasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BasketDto>> RemoveItem(int productId)
    {
        if (!_userContext.IsAuthenticated || UserId == null)
        {
            return Unauthorized();
        }

        _logger.LogInformation("Removing item {ProductId} from basket for user {UserId}", productId, UserId);

        var basket = await _basketService.RemoveItemFromBasketAsync(UserId, productId);

        if (basket == null)
        {
            _logger.LogWarning("Item {ProductId} not found in basket for user {UserId}", productId, UserId);
            return NotFound(new { message = $"Item {productId} not found in basket for user {UserId}" });
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
