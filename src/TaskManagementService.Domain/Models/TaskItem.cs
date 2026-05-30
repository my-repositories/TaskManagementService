namespace TaskManagementService.Domain.Models;

public class TaskItem
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}