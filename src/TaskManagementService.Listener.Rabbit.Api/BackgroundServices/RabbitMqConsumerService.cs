using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using TaskManagementService.Domain.Configurations;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Listener.Rabbit.Api.Models;

namespace TaskManagementService.Listener.Rabbit.Api.BackgroundServices;

/// <summary>
/// Фоновый сервис для непрерывного чтения и распределения интеграционных сообщений из очереди RabbitMQ.
/// </summary>
public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly RabbitMqOptions _rabbitConfig;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IChannel? _channel;

    /// <summary>
    /// Инициализирует новый экземпляр фонового сервиса RabbitMqConsumerService.
    /// </summary>
    /// <param name="logger">Компонент для логирования.</param>
    /// <param name="options">Конфигурационные параметры подключения к RabbitMQ.</param>
    /// <param name="scopeFactory">Фабрика для управления контекстами зависимостей Scoped.</param>
    public RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _rabbitConfig = options.Value;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Запускает асинхронный процесс прослушивания очереди брокера сообщений.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены выполнения фонового процесса.</param>
    /// <returns>Задача, представляющая асинхронное выполнение воркера.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var factory = new ConnectionFactory
        {
            HostName = _rabbitConfig.Host,
            UserName = _rabbitConfig.UserName,
            Password = _rabbitConfig.Password
        };

        int retryCount = 0;
        int maxRetries = 10;

        while (_connection == null && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("[RabbitMqConsumerService] Попытка подключения к RabbitMQ ({Attempt}/{Max})...", retryCount + 1, maxRetries);
                
                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                
                _logger.LogInformation("[RabbitMqConsumerService] Успешно подключено к RabbitMQ и создан канал.");
            }
            catch (BrokerUnreachableException ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger.LogCritical(ex, "[RabbitMqConsumerService] Брокер недоступен после {Max} попыток. Падаем.", maxRetries);
                    throw;
                }

                _logger.LogWarning("[RabbitMqConsumerService] Порт RabbitMQ еще закрыт. Ожидание 3 секунды...");
                await Task.Delay(3000, stoppingToken);
            }
        }

        if (_channel == null) return;

        await _channel.QueueDeclareAsync(
            queue: _rabbitConfig.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jsonString = Encoding.UTF8.GetString(body);

            try
            {
                var message = JsonSerializer.Deserialize<TaskEventMessage>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (message?.Data != null && !string.IsNullOrEmpty(message.Action))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var handlers = scope.ServiceProvider.GetServices<ITaskEventHandler>();
                    var targetHandler = handlers.FirstOrDefault(h => h.ActionName.Equals(message.Action, StringComparison.OrdinalIgnoreCase));

                    if (targetHandler != null)
                    {
                        await targetHandler.HandleAsync(message.Data, stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("[RabbitMqConsumerService] Не найден обработчик для события: {Action}", message.Action);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMqConsumerService] Ошибка при обработке сообщения из очереди");
            }
            finally
            {
                if (_channel is { IsOpen: true })
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _rabbitConfig.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
            
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Освобождает неуправляемые ресурсы, используемые для подключения к RabbitMQ.
    /// </summary>
    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
