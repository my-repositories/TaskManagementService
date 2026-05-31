using Microsoft.Extensions.Logging;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Rabbit.Api.Handlers;

/// <summary>
/// Обработчик интеграционного события создания новой задачи.
/// </summary>
/// <param name="logger">Компонент для логирования событий.</param>
public class TaskCreatedEventHandler(ILogger<TaskCreatedEventHandler> logger) : ITaskEventHandler
{
    /// <summary>Имя обрабатываемого события ("Created").</summary>
    public string ActionName => TaskEvent.Created.ToString();

    /// <summary>
    /// Выполняет логирование информации о созданной задаче.
    /// </summary>
    /// <param name="task">Объект созданной задачи.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки.</returns>
    public async Task HandleAsync(TaskItem task, CancellationToken cancellationToken)
    {
        logger.LogInformation("создание задачи: {Title}, Статус: {Status}", task.Id, task.Title, task.Status);
    }
}
