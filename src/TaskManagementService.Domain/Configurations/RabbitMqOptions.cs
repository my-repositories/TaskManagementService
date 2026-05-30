namespace TaskManagementService.Domain.Configurations;

/// <summary>
/// Параметры конфигурации подключения к брокеру сообщений RabbitMQ.
/// </summary>
public class RabbitMqOptions
{
    /// <summary>Хост или IP-адрес сервера брокера сообщений.</summary>
    public required string Host { get; set; }

    /// <summary>Наименование целевой очереди для отправки или чтения событий.</summary>
    public required string QueueName { get; set; }

    /// <summary>Имя пользователя для авторизации в системе брокера.</summary>
    public required string UserName { get; set; }

    /// <summary>Пароль для авторизации в системе брокера.</summary>
    public required string Password { get; set; }
}
