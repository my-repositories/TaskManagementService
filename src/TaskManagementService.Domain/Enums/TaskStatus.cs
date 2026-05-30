namespace TaskManagementService.Domain.Enums;

/// <summary>
/// Возможные статусы выполнения задачи пользователя.
/// </summary>
public enum TaskStatus
{
    /// <summary>Задача создана и ожидает начала выполнения.</summary>
    New,

    /// <summary>Задача находится в процессе выполнения.</summary>
    InProgress,

    /// <summary>Задача успешно завершена.</summary>
    Completed
}
