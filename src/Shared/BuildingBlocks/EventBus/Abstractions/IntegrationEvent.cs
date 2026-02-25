using System.Text.Json.Serialization;

namespace EventBus.Abstractions;

public abstract record IntegrationEvent
{
    [JsonInclude]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonInclude]
    public DateTime CreationDate { get; init; } = DateTime.UtcNow;
}
