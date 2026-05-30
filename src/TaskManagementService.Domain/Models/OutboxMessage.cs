namespace TaskManagementService.Domain.Models;

/// <summary>
/// Сущность отложенного интеграционного сообщения для реализации паттерна Transactional Outbox.
/// </summary>
public class OutboxMessage
{
    /// <summary>Уникальный идентификатор сообщения.</summary>
    public Guid Id { get; set; }

    /// <summary>Тип произошедшего доменного события.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Сериализованные в JSON данные доменного объекта.</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>Дата и время фиксации события (UTC).</summary>
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

    /// <summary>Дата и время успешной обработки сообщения воркером (UTC).</summary>
    public DateTime? ProcessedOn { get; set; }

    /// <summary>Текст ошибки в случае сбоя при отправке сообщения.</summary>
    public string? Error { get; set; }
}
