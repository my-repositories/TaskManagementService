using TaskManagementService.Domain.Models;

namespace TaskManagementService.Domain.Interfaces;

/// <summary>
/// Интерфейс службы отправки интеграционных уведомлений об изменениях задач.
/// </summary>
public interface ITaskEventService
{
    /// <summary>
    /// Распределяет уведомление об изменении задачи по внешним каналам связи.
    /// </summary>
    /// <param name="action">Тип операции (создание, изменение, удаление).</param>
    /// <param name="task">Объект задачи, с которым связано событие.</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки.</returns>
    Task NotifyAsync(string action, TaskItem task);
}
