using AutoMapper;
using Catalog.API.DTOs;
using Catalog.API.Models;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository repository,
        IMapper mapper,
        ILogger<ProductsController> logger)
    {
        _repository = repository;
        _mapper = mapper;
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
        var products = await _repository.GetAllAsync();
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(productDtos);
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
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product with id {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
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
        var products = await _repository.GetByCategoryIdAsync(categoryId);
        var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(productDtos);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createProductDto">Product data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createProductDto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", createProductDto.Name);

        // Validate category exists
        if (!await _repository.CategoryExistsAsync(createProductDto.CategoryId))
        {
            _logger.LogWarning("Category with id {CategoryId} not found", createProductDto.CategoryId);
            return BadRequest(new { message = $"Category with id {createProductDto.CategoryId} does not exist" });
        }

        var product = _mapper.Map<Product>(createProductDto);
        var createdProduct = await _repository.CreateAsync(product);
        var productDto = _mapper.Map<ProductDto>(createdProduct);

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        _logger.LogInformation("Updating product with id {ProductId}", id);

        var existingProduct = await _repository.GetByIdAsync(id);
        if (existingProduct == null)
        {
            _logger.LogWarning("Product with id {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        // Validate category exists
        if (!await _repository.CategoryExistsAsync(updateProductDto.CategoryId))
        {
            _logger.LogWarning("Category with id {CategoryId} not found", updateProductDto.CategoryId);
            return BadRequest(new { message = $"Category with id {updateProductDto.CategoryId} does not exist" });
        }

        _mapper.Map(updateProductDto, existingProduct);
        existingProduct.Id = id; // Ensure ID doesn't change

        var updatedProduct = await _repository.UpdateAsync(existingProduct);
        var productDto = _mapper.Map<ProductDto>(updatedProduct);

        return Ok(productDto);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting product with id {ProductId}", id);

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Product with id {ProductId} not found", id);
            return NotFound(new { message = $"Product with id {id} not found" });
        }

        return NoContent();
    }
}
