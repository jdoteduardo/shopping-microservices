using Basket.API.DTOs;

namespace Basket.API.Services;

public interface IBasketService
{
    Task<BasketDto?> GetBasketAsync(string userId);
    Task<BasketDto?> UpdateBasketAsync(string userId, UpdateBasketDto updateBasketDto);
    Task<bool> DeleteBasketAsync(string userId);
    Task<BasketDto?> AddItemToBasketAsync(string userId, AddItemDto addItemDto);
    Task<BasketDto?> UpdateItemQuantityAsync(string userId, int productId, int quantity);
    Task<BasketDto?> RemoveItemFromBasketAsync(string userId, int productId);
}
