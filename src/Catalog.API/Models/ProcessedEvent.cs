using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Models;

/// <summary>
/// To ensure idempotency, we track processed integration events.
/// </summary>
public class ProcessedEvent
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string EventType { get; set; } = string.Empty;
    
    public DateTime ProcessedAt { get; set; }
}
