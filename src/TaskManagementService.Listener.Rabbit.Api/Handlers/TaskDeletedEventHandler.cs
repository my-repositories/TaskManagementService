using Microsoft.Extensions.Logging;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Rabbit.Api.Handlers;

/// <summary>
/// Обработчик интеграционного события удаления задачи.
/// </summary>
/// <param name="logger">Компонент для логирования событий.</param>
public class TaskDeletedEventHandler(ILogger<TaskDeletedEventHandler> logger) : ITaskEventHandler
{
    /// <summary>Имя обрабатываемого события ("Deleted").</summary>
    public string ActionName => TaskEvent.Deleted.ToString();

    /// <summary>
    /// Выполняет логирование информации об удаленной задаче.
    /// </summary>
    /// <param name="task">Объект удаленной задачи.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки.</returns>
    public async Task HandleAsync(TaskItem task, CancellationToken cancellationToken)
    {
        logger.LogInformation("удаление задачи #{Id}: {Title}, Статус: {Status}", task.Id, task.Title, task.Status);
    }
}
