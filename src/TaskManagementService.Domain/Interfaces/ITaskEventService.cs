using TaskManagementService.Domain.Models;

namespace TaskManagementService.Domain.Interfaces;

public interface ITaskEventService
{
    Task NotifyAsync(string action, TaskItem task);
}
