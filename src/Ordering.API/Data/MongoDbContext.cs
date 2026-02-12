using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Ordering.API.Models;

namespace Ordering.API.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);

        CreateIndexes();
    }

    public IMongoCollection<Order> Orders => 
        _database.GetCollection<Order>(_settings.OrdersCollectionName);

    private void CreateIndexes()
    {
        var ordersCollection = Orders;

        // Unique index on OrderNumber
        var orderNumberIndex = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(o => o.OrderNumber),
            new CreateIndexOptions { Unique = true });

        // Index on UserId for faster user order queries
        var userIdIndex = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(o => o.UserId));

        // Index on OrderDate for sorting and filtering
        var orderDateIndex = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Descending(o => o.OrderDate));

        // Compound index on UserId and OrderDate
        var userIdOrderDateIndex = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys
                .Ascending(o => o.UserId)
                .Descending(o => o.OrderDate));

        ordersCollection.Indexes.CreateMany(new[] 
        { 
            orderNumberIndex, 
            userIdIndex, 
            orderDateIndex,
            userIdOrderDateIndex 
        });
    }
}
