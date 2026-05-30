using TaskManagementService.Domain.Models;

namespace TaskManagementService.Domain.Interfaces;

/// <summary>
/// Интерфейс обработчика интеграционных событий изменения задач.
/// </summary>
public interface ITaskEventHandler
{
    /// <summary>Наименование типа события, обрабатываемого данным классом.</summary>
    string ActionName { get; }

    /// <summary>
    /// Выполняет логику обработки интеграционного события задачи.
    /// </summary>
    /// <param name="task">Объект задачи, полученный из внешнего источника.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки.</returns>
    Task HandleAsync(TaskItem task, CancellationToken cancellationToken);
}
