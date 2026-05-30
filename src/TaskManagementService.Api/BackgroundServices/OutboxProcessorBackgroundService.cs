using Microsoft.EntityFrameworkCore;
using TaskManagementService.Dal;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;
using System.Text.Json;

namespace TaskManagementService.Api.BackgroundServices;

public class OutboxProcessorBackgroundService(
    IServiceScopeFactory scopeFactory,
    ITaskEventService eventService,
    ILogger<OutboxProcessorBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var messages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedOn == null)
                    .OrderBy(m => m.OccurredOn)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        var task = JsonSerializer.Deserialize<TaskItem>(message.Payload);
                        if (task != null)
                        {
                            await eventService.NotifyAsync(message.Action, task);
                        }

                        message.ProcessedOn = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Ошибка отправки сообщения {Id}", message.Id);
                        message.Error = ex.Message; 
                    }
                }

                if (messages.Count > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ошибка в обработке сообщения");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
