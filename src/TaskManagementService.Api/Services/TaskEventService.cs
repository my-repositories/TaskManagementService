using System.Text;
using System.Text.Json;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Api.Services;

public class TaskEventService(HttpClient httpClient, IConfiguration configuration, ILogger<TaskEventService> logger) : ITaskEventService
{
    private readonly string _httpBaseUrl = configuration["ExternalServices:HttpListenerUrl"] 
        ?? throw new ArgumentNullException(null, nameof(_httpBaseUrl));

    public async Task NotifyAsync(string action, TaskItem task)
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
}
