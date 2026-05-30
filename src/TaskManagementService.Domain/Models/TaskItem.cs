namespace TaskManagementService.Domain.Models;

/// <summary>
/// Доменная модель задачи пользователя.
/// </summary>
public class TaskItem
{
    /// <summary>Уникальный идентификатор задачи.</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор пользователя, которому принадлежит задача.</summary>
    public required string UserId { get; set; }

    /// <summary>Наименование задачи.</summary>
    public required string Title { get; set; }

    /// <summary>Подробное описание или заметки по задаче.</summary>
    public string? Description { get; set; }

    /// <summary>Текущий статус выполнения задачи.</summary>
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.New;

    /// <summary>Дата и время создания записи (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата и время последнего обновления параметров записи (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
