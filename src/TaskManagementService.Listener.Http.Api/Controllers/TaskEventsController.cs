using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Listener.Http.Api.Controllers;

[ApiController]
[Route("api/tasks/events")]
public class TaskEventsController(ILogger<TaskEventsController> logger) : ControllerBase
{
    [HttpPost]
    public IActionResult HandleEvent([FromQuery] string action, [FromBody] TaskItem task)
    {
        var payload = JsonSerializer.Serialize(task);
        
        logger.LogInformation("Получено событие [{Action}]: {Payload}", action.ToUpper(), payload);
        
        return Ok();
    }
}
