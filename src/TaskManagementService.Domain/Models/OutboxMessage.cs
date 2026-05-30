namespace TaskManagementService.Domain.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
}
