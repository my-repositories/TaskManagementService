namespace TaskManagementService.Domain.Enums;

/// <summary>
/// Типы интеграционных событий изменений задач.
/// </summary>
public enum TaskEvent
{
    /// <summary>Событие создания новой задачи.</summary>
    Created,

    /// <summary>Событие обновления параметров задачи.</summary>
    Updated,

    /// <summary>Событие удаления задачи.</summary>
    Deleted
}
