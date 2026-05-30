using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Rabbit.Api.Models;

/// <summary>
/// Модель интеграционного сообщения, получаемого из брокера RabbitMQ.
/// </summary>
public class TaskEventMessage
{
    /// <summary>Наименование произошедшего события.</summary>
    public string? Action { get; set; }

    /// <summary>Данные измененной задачи.</summary>
    public TaskItem? Data { get; set; }
}
