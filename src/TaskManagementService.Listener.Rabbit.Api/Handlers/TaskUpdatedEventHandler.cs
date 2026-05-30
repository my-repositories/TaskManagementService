using Microsoft.Extensions.Logging;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Rabbit.Api.Handlers;

public class TaskUpdatedEventHandler(ILogger<TaskUpdatedEventHandler> logger) : ITaskEventHandler
{
    public string ActionName => TaskEvent.Updated.ToString();

    public async Task HandleAsync(TaskItem task, CancellationToken cancellationToken)
    {
        logger.LogInformation("обновление задачи #{Id}: {Title}, Статус: {Status}", task.Id, task.Title, task.Status);
    }
}
