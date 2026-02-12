using Basket.API.Models;

namespace Basket.API.Repositories;

public interface IBasketRepository
{
    Task<Models.Basket?> GetBasketAsync(string userId);
    Task<Models.Basket?> UpdateBasketAsync(Models.Basket basket);
    Task<bool> DeleteBasketAsync(string userId);
}
