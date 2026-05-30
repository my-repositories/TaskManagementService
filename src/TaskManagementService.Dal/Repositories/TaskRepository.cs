using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagementService.Domain.Enums;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Dal.Repositories;

/// <summary>
/// Репозиторий для управления сущностями задач с поддержкой паттерна Transactional Outbox.
/// </summary>
/// <param name="context">Контекст базы данных.</param>
public class TaskRepository(AppDbContext context) : ITaskRepository
{
    /// <summary>
    /// Возвращает коллекцию всех задач конкретного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Список задач пользователя.</returns>
    public async Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(string userId) =>
        await context.Tasks.Where(t => t.UserId == userId).ToListAsync();

    /// <summary>
    /// Находит задачу по её идентификатору, проверяя принадлежность пользователю.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Экземпляр задачи или null, если задача не найдена.</returns>
    public async Task<TaskItem?> GetByIdAndUserAsync(int id, string userId) =>
        await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    /// <summary>
    /// Добавляет новую задачу в базу данных и регистрирует сообщение в таблице Outbox.
    /// </summary>
    /// <param name="task">Объект создаваемой задачи.</param>
    public async Task AddAsync(TaskItem task)
    {
        context.Tasks.Add(task);
        EnqueueOutbox(TaskEvent.Created.ToString(), task);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Обновляет существующую задачу в базе данных и регистрирует сообщение в таблице Outbox.
    /// </summary>
    /// <param name="task">Объект обновляемой задачи.</param>
    public async Task UpdateAsync(TaskItem task)
    {
        context.Tasks.Update(task);
        EnqueueOutbox(TaskEvent.Updated.ToString(), task);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Удаляет задачу из базы данных и регистрирует сообщение в таблице Outbox.
    /// </summary>
    /// <param name="task">Объект удаляемой задачи.</param>
    public async Task DeleteAsync(TaskItem task)
    {
        context.Tasks.Remove(task);
        EnqueueOutbox(TaskEvent.Deleted.ToString(), task);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Добавляет запись интеграционного события в таблицу отложенных сообщений Outbox.
    /// </summary>
    /// <param name="action">Тип события.</param>
    /// <param name="task">Объект задачи, связанный с событием.</param>
    private void EnqueueOutbox(string action, TaskItem task)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Action = action,
            Payload = JsonSerializer.Serialize(task),
            OccurredOn = DateTime.UtcNow
        };
        context.OutboxMessages.Add(outboxMessage);
    }
}
