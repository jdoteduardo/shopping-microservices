using Catalog.API.DTOs;
using Catalog.API.Services;
using Microsoft.AspNetCore.Mvc;
using UserContext;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IUserContext _userContext;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IUserContext userContext,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of all products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        _logger.LogInformation("Getting all products");
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        _logger.LogInformation("Getting product with id {ProductId}", id);
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product with id {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Get products by category ID
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of products in the category</returns>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategoryId(int categoryId)
    {
        _logger.LogInformation("Getting products for category {CategoryId}", categoryId);
        var products = await _productService.GetByCategoryIdAsync(categoryId);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createProductDto">Product data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createProductDto)
    {
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized();
        }

        _logger.LogInformation("User {UserId} is creating new product: {ProductName}", 
            _userContext.UserId, createProductDto.Name);

        var productDto = await _productService.CreateAsync(createProductDto, _userContext.UserId);
        return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateProductDto">Updated product data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        if (!_userContext.IsAuthenticated)
        {
            return Unauthorized();
        }

        _logger.LogInformation("User {UserId} is updating product with id {ProductId}", 
            _userContext.UserId, id);

        var productDto = await _productService.UpdateAsync(id, updateProductDto, _userContext.UserId);
        if (productDto == null)
        {
            _logger.LogWarning("Product with id {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        return Ok(productDto);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!_userContext.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        _logger.LogWarning("Product {ProductId} deleted by admin {UserId}", id, _userContext.UserId);

        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Product with id {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        return NoContent();
    }
}
