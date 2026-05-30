using TaskManagementService.Domain.Models;

namespace TaskManagementService.Domain.Interfaces;

public interface ITaskEventHandler
{
    string ActionName { get; }
    
    Task HandleAsync(TaskItem task, CancellationToken cancellationToken);
}
