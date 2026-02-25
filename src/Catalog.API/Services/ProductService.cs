using AutoMapper;
using EventBus.Abstractions;
using IntegrationEvents;
using Catalog.API.DTOs;
using Catalog.API.Models;
using Catalog.API.Repositories;

namespace Catalog.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IMapper mapper,
        IEventBus eventBus,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(int categoryId)
    {
        var products = await _repository.GetByCategoryIdAsync(categoryId);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto, string? userId)
    {
        var product = _mapper.Map<Product>(createProductDto);
        product.CreatedAt = DateTime.UtcNow;
        product.CreatedBy = userId;
        var created = await _repository.CreateAsync(product);
        return _mapper.Map<ProductDto>(created);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto updateProductDto, string? userId)
    {
        var existingProduct = await _repository.GetByIdAsync(id);
        if (existingProduct == null)
            return null;

        var oldPrice = existingProduct.Price;

        _mapper.Map(updateProductDto, existingProduct);
        existingProduct.Id = id; // Ensure ID doesn't change
        existingProduct.UpdatedAt = DateTime.UtcNow;
        existingProduct.UpdatedBy = userId;

        var updatedProduct = await _repository.UpdateAsync(existingProduct);
        var productDto = _mapper.Map<ProductDto>(updatedProduct);

        // Publish ProductPriceChangedIntegrationEvent if price changed
        if (oldPrice != updatedProduct.Price)
        {
            try
            {
                _logger.LogInformation(
                    "Product {ProductId} ({ProductName}) price changed from {OldPrice} to {NewPrice}. Publishing event.",
                    updatedProduct.Id, updatedProduct.Name, oldPrice, updatedProduct.Price);

                var priceChangedEvent = new ProductPriceChangedIntegrationEvent(
                    productId: updatedProduct.Id,
                    productName: updatedProduct.Name,
                    oldPrice: oldPrice,
                    newPrice: updatedProduct.Price
                );

                await _eventBus.PublishAsync(priceChangedEvent);

                _logger.LogInformation("Successfully published ProductPriceChangedIntegrationEvent for product {ProductId}", updatedProduct.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish ProductPriceChangedIntegrationEvent for product {ProductId}", updatedProduct.Id);
            }
        }

        return productDto;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
