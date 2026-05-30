using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Rabbit.Api.Models;

public class TaskEventMessage
{
    public string? Action { get; set; }
    public TaskItem? Data { get; set; }
}
