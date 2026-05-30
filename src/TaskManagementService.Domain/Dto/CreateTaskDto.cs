namespace TaskManagementService.Domain.Dto;

/// <summary>
/// Данные для создания новой задачи.
/// </summary>
public class CreateTaskDto
{
    /// <summary>Наименование задачи.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Описание задачи.</summary>
    public string? Description { get; set; }
}
