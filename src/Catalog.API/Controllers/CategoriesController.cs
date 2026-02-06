using AutoMapper;
using Catalog.API.DTOs;
using Catalog.API.Models;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryRepository repository,
        IMapper mapper,
        ILogger<CategoriesController> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns>List of all categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        _logger.LogInformation("Getting all categories");
        var categories = await _repository.GetAllAsync();
        var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
        return Ok(categoryDtos);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        _logger.LogInformation("Getting category with id {CategoryId}", id);
        var category = await _repository.GetByIdAsync(id);

        if (category == null)
        {
            _logger.LogWarning("Category with id {CategoryId} not found", id);
            return NotFound(new { message = $"Category with id {id} not found" });
        }

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(categoryDto);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="createCategoryDto">Category data</param>
    /// <returns>Created category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto createCategoryDto)
    {
        _logger.LogInformation("Creating new category: {CategoryName}", createCategoryDto.Name);

        var category = _mapper.Map<Category>(createCategoryDto);
        var createdCategory = await _repository.CreateAsync(category);
        var categoryDto = _mapper.Map<CategoryDto>(createdCategory);

        return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="updateCategoryDto">Updated category data</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        _logger.LogInformation("Updating category with id {CategoryId}", id);

        var existingCategory = await _repository.GetByIdAsync(id);
        if (existingCategory == null)
        {
            _logger.LogWarning("Category with id {CategoryId} not found", id);
            return NotFound(new { message = $"Category with id {id} not found" });
        }

        _mapper.Map(updateCategoryDto, existingCategory);
        existingCategory.Id = id; // Ensure ID doesn't change

        var updatedCategory = await _repository.UpdateAsync(existingCategory);
        var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);

        return Ok(categoryDto);
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting category with id {CategoryId}", id);

        // Check if category has products
        if (await _repository.HasProductsAsync(id))
        {
            _logger.LogWarning("Cannot delete category {CategoryId} because it has products", id);
            return BadRequest(new { message = "Cannot delete category that has products" });
        }

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Category with id {CategoryId} not found", id);
            return NotFound(new { message = $"Category with id {id} not found" });
        }

        return NoContent();
    }
}
