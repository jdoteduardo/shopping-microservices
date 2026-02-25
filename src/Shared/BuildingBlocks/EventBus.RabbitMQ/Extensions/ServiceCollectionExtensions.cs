using EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, RabbitMQSettings settings, string clientName = "shop_client")
    {
        services.AddSingleton(settings);

        services.AddSingleton<IEventBusSubscriptionsManager, EventBusSubscriptionsManager>();

        services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQConnection>>();
            var factory = new ConnectionFactory()
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,
                DispatchConsumersAsync = true
            };

            return new RabbitMQConnection(factory, logger, settings.RetryCount);
        });

        services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        {
            var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
            var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
            var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

            return new EventBusRabbitMQ(
                rabbitMQPersistentConnection, 
                logger, 
                serviceScopeFactory, 
                eventBusSubcriptionsManager, 
                queueName: clientName, 
                retryCount: settings.RetryCount);
        });

        return services;
    }
}
