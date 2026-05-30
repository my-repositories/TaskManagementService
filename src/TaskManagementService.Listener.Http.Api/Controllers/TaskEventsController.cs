using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Http.Api.Controllers;

/// <summary>
/// Контроллер для обработки входящих REST-уведомлений об изменениях задач.
/// </summary>
/// <param name="logger">Компонент для логирования событий.</param>
[ApiController]
[Route("api/tasks/events")]
public class TaskEventsController(ILogger<TaskEventsController> logger) : ControllerBase
{
    /// <summary>
    /// Принимает POST-запрос с событием изменения задачи и логирует его.
    /// </summary>
    /// <param name="action">Тип события (создание, изменение, удаление).</param>
    /// <param name="task">Объект измененной задачи.</param>
    /// <returns>Статус Ok в случае успешного логирования.</returns>
    [HttpPost]
    public IActionResult HandleEvent([FromQuery] string action, [FromBody] TaskItem task)
    {
        var payload = JsonSerializer.Serialize(task);

        logger.LogInformation("Получено событие [{Action}]: {Payload}", action.ToUpper(), payload);

        return Ok();
    }
}
