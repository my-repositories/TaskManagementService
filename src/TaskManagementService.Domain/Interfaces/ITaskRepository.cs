using TaskManagementService.Domain.Models;

namespace TaskManagementService.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория для управления хранилищем задач.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Возвращает все задачи конкретного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Коллекция задач пользователя.</returns>
    Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(string userId);

    /// <summary>
    /// Находит задачу по её идентификатору с проверкой принадлежности пользователю.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Экземпляр найденной задачи или null.</returns>
    Task<TaskItem?> GetByIdAndUserAsync(int id, string userId);

    /// <summary>
    /// Сохраняет новую задачу.
    /// </summary>
    /// <param name="task">Объект создаваемой задачи.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task AddAsync(TaskItem task);

    /// <summary>
    /// Обновляет существующую задачу.
    /// </summary>
    /// <param name="task">Объект обновляемой задачи.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task UpdateAsync(TaskItem task);

    /// <summary>
    /// Удаляет задачу.
    /// </summary>
    /// <param name="task">Объект удаляемой задачи.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task DeleteAsync(TaskItem task);
}
