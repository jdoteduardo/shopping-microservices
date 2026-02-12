using System.Text.Json;
using StackExchange.Redis;

namespace Basket.API.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _database;
    private readonly ILogger<BasketRepository> _logger;
    private readonly TimeSpan _basketTtl = TimeSpan.FromHours(24);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public BasketRepository(IConnectionMultiplexer redis, ILogger<BasketRepository> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<Models.Basket?> GetBasketAsync(string userId)
    {
        _logger.LogInformation("Getting basket for user {UserId}", userId);

        var data = await _database.StringGetAsync(GetKey(userId));

        if (data.IsNullOrEmpty)
        {
            _logger.LogInformation("Basket not found for user {UserId}", userId);
            return null;
        }

        try
        {
            var basket = JsonSerializer.Deserialize<Models.Basket>(data!, JsonOptions);
            _logger.LogInformation("Basket retrieved for user {UserId} with {ItemCount} items", 
                userId, basket?.Items.Count ?? 0);
            return basket;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing basket for user {UserId}", userId);
            return null;
        }
    }

    public async Task<Models.Basket?> UpdateBasketAsync(Models.Basket basket)
    {
        _logger.LogInformation("Updating basket for user {UserId} with {ItemCount} items", 
            basket.UserId, basket.Items.Count);

        var json = JsonSerializer.Serialize(basket, JsonOptions);
        var created = await _database.StringSetAsync(
            GetKey(basket.UserId), 
            json, 
            _basketTtl);

        if (!created)
        {
            _logger.LogError("Failed to update basket for user {UserId}", basket.UserId);
            return null;
        }

        _logger.LogInformation("Basket updated successfully for user {UserId}", basket.UserId);
        return await GetBasketAsync(basket.UserId);
    }

    public async Task<bool> DeleteBasketAsync(string userId)
    {
        _logger.LogInformation("Deleting basket for user {UserId}", userId);

        var deleted = await _database.KeyDeleteAsync(GetKey(userId));
        
        if (deleted)
        {
            _logger.LogInformation("Basket deleted successfully for user {UserId}", userId);
        }
        else
        {
            _logger.LogWarning("Basket not found for deletion, user {UserId}", userId);
        }

        return deleted;
    }

    private static string GetKey(string userId) => $"basket:{userId}";
}
