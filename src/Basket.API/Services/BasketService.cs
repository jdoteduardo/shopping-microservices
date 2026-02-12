using AutoMapper;
using Basket.API.DTOs;
using Basket.API.Models;
using Basket.API.Repositories;

namespace Basket.API.Services;

public class BasketService : IBasketService
{
    private readonly IBasketRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<BasketService> _logger;

    public BasketService(
        IBasketRepository repository,
        IMapper mapper,
        ILogger<BasketService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BasketDto?> GetBasketAsync(string userId)
    {
        _logger.LogInformation("Service: Getting basket for user {UserId}", userId);
        
        var basket = await _repository.GetBasketAsync(userId);
        
        if (basket == null)
        {
            // Return empty basket for new users
            return new BasketDto
            {
                UserId = userId,
                Items = new List<BasketItemDto>(),
                TotalPrice = 0
            };
        }

        return _mapper.Map<BasketDto>(basket);
    }

    public async Task<BasketDto?> UpdateBasketAsync(UpdateBasketDto updateBasketDto)
    {
        _logger.LogInformation("Service: Updating basket for user {UserId}", updateBasketDto.UserId);

        var basket = _mapper.Map<Models.Basket>(updateBasketDto);
        var updatedBasket = await _repository.UpdateBasketAsync(basket);

        return updatedBasket != null ? _mapper.Map<BasketDto>(updatedBasket) : null;
    }

    public async Task<bool> DeleteBasketAsync(string userId)
    {
        _logger.LogInformation("Service: Deleting basket for user {UserId}", userId);
        return await _repository.DeleteBasketAsync(userId);
    }

    public async Task<BasketDto?> AddItemToBasketAsync(string userId, AddItemDto addItemDto)
    {
        _logger.LogInformation("Service: Adding item {ProductId} to basket for user {UserId}", 
            addItemDto.ProductId, userId);

        var basket = await _repository.GetBasketAsync(userId) ?? new Models.Basket { UserId = userId };

        // Check if item already exists
        var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == addItemDto.ProductId);

        if (existingItem != null)
        {
            // Update quantity if item exists
            existingItem.Quantity += addItemDto.Quantity;
            existingItem.Price = addItemDto.Price; // Update price in case it changed
            existingItem.ProductName = addItemDto.ProductName;
            _logger.LogInformation("Updated existing item {ProductId}, new quantity: {Quantity}", 
                addItemDto.ProductId, existingItem.Quantity);
        }
        else
        {
            // Add new item
            var newItem = _mapper.Map<BasketItem>(addItemDto);
            basket.Items.Add(newItem);
            _logger.LogInformation("Added new item {ProductId} to basket", addItemDto.ProductId);
        }

        var updatedBasket = await _repository.UpdateBasketAsync(basket);
        return updatedBasket != null ? _mapper.Map<BasketDto>(updatedBasket) : null;
    }

    public async Task<BasketDto?> UpdateItemQuantityAsync(string userId, int productId, int quantity)
    {
        _logger.LogInformation("Service: Updating quantity for item {ProductId} in basket for user {UserId}", 
            productId, userId);

        var basket = await _repository.GetBasketAsync(userId);

        if (basket == null)
        {
            _logger.LogWarning("Basket not found for user {UserId}", userId);
            return null;
        }

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
        {
            _logger.LogWarning("Item {ProductId} not found in basket for user {UserId}", productId, userId);
            return null;
        }

        if (quantity <= 0)
        {
            // Remove item if quantity is 0 or negative
            basket.Items.Remove(item);
            _logger.LogInformation("Removed item {ProductId} from basket (quantity <= 0)", productId);
        }
        else
        {
            item.Quantity = quantity;
            _logger.LogInformation("Updated item {ProductId} quantity to {Quantity}", productId, quantity);
        }

        var updatedBasket = await _repository.UpdateBasketAsync(basket);
        return updatedBasket != null ? _mapper.Map<BasketDto>(updatedBasket) : null;
    }

    public async Task<BasketDto?> RemoveItemFromBasketAsync(string userId, int productId)
    {
        _logger.LogInformation("Service: Removing item {ProductId} from basket for user {UserId}", 
            productId, userId);

        var basket = await _repository.GetBasketAsync(userId);

        if (basket == null)
        {
            _logger.LogWarning("Basket not found for user {UserId}", userId);
            return null;
        }

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
        {
            _logger.LogWarning("Item {ProductId} not found in basket for user {UserId}", productId, userId);
            return null;
        }

        basket.Items.Remove(item);
        _logger.LogInformation("Removed item {ProductId} from basket", productId);

        var updatedBasket = await _repository.UpdateBasketAsync(basket);
        return updatedBasket != null ? _mapper.Map<BasketDto>(updatedBasket) : null;
    }
}
