namespace TaskManagementService.Domain.Dto;

/// <summary>
/// Данные для обновления существующей задачи.
/// </summary>
public class UpdateTaskDto
{
    /// <summary>Новое наименование задачи.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Новое описание задачи.</summary>
    public string? Description { get; set; }

    /// <summary>Новый статус выполнения задачи.</summary>
    public Enums.TaskStatus Status { get; set; }
}
