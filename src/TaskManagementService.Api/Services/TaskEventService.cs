using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaskManagementService.Domain.Configurations;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Api.Services;

public class TaskEventService
(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<TaskEventService> logger,
    IOptions<RabbitMqOptions> rabbitOptions
) : ITaskEventService
{
    private readonly string _httpBaseUrl = configuration["ExternalServices:HttpListenerUrl"]
        ?? throw new ArgumentNullException(null, nameof(_httpBaseUrl));

    private readonly RabbitMqOptions _rabbitConfig = rabbitOptions.Value;


    public async Task NotifyAsync(string action, TaskItem task)
    {
        await Task.WhenAll
        (
            NotifyHttpListenerAsync(action, task),
            NotifyMqListenerAsync(action, task)
        );
    }

    public async Task NotifyHttpListenerAsync(string action, TaskItem task)
    {
        var payload = JsonSerializer.Serialize(task);

        try
        {
            var httpUrl = $"{_httpBaseUrl}?action={action}";
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(httpUrl, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError("Ошибка синхронной отправки HTTP: {Message}", ex.Message);
        }
    }

    public async Task NotifyMqListenerAsync(string action, TaskItem task)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitConfig.Host,
                UserName = _rabbitConfig.UserName,
                Password = _rabbitConfig.Password
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: _rabbitConfig.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var eventMessage = new { Action = action, Data = task };
            var messageBody = JsonSerializer.Serialize(eventMessage);
            var body = Encoding.UTF8.GetBytes(messageBody);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _rabbitConfig.QueueName,
                body: body);
        }
        catch (Exception ex)
        {
            logger.LogError("Ошибка асинхронной отправки в RabbitMQ: {Message}", ex.Message);
        }
    }
}
