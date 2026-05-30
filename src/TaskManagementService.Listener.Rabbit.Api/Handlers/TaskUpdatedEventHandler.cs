using Microsoft.Extensions.Logging;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Rabbit.Api.Handlers;

/// <summary>
/// Обработчик интеграционного события обновления задачи.
/// </summary>
/// <param name="logger">Компонент для логирования событий.</param>
public class TaskUpdatedEventHandler(ILogger<TaskUpdatedEventHandler> logger) : ITaskEventHandler
{
    /// <summary>Имя обрабатываемого события ("Updated").</summary>
    public string ActionName => TaskEvent.Updated.ToString();

    /// <summary>
    /// Выполняет логирование информации об обновленной задаче.
    /// </summary>
    /// <param name="task">Объект обновленной задачи.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки.</returns>
    public async Task HandleAsync(TaskItem task, CancellationToken cancellationToken)
    {
        logger.LogInformation("обновление задачи #{Id}: {Title}, Статус: {Status}", task.Id, task.Title, task.Status);
    }
}
