using Catalog.API.DTOs;

namespace Catalog.API.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(int categoryId);
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto, string? userId);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductDto updateProductDto, string? userId);
    Task<bool> DeleteAsync(int id);
}
