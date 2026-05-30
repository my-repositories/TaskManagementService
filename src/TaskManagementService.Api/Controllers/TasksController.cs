using Microsoft.AspNetCore.Mvc;
using TaskManagementService.Domain.Dto;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Api.Controllers;

/// <summary>
/// Контроллер для управления задачами пользователя.
/// </summary>
/// <param name="taskRepository">Репозиторий для работы с задачами.</param>
[ApiController]
[Route("api/[controller]")]
public class TasksController(ITaskRepository taskRepository) : ControllerBase
{
    /// <summary>
    /// Идентификатор текущего пользователя.
    /// Временная заглушка для симуляции контекста пользователя без подключения системы аутентификации.
    /// </summary>
    private string CurrentUserId => "1";

    /// <summary>
    /// Получает список всех задач текущего пользователя.
    /// </summary>
    /// <returns>Коллекция задач пользователя.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll()
    {
        var tasks = await taskRepository.GetAllByUserIdAsync(CurrentUserId);
        return Ok(tasks);
    }

    /// <summary>
    /// Получает задачу по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <returns>Модель найденной задачи или NotFound.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await taskRepository.GetByIdAndUserAsync(id, CurrentUserId);
        if (task is null) return NotFound();
        return Ok(task);
    }

    /// <summary>
    /// Создает новую задачу.
    /// </summary>
    /// <param name="dto">Данные для создания задачи.</param>
    /// <returns>Созданная задача с присвоенным идентификатором.</returns>
    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            UserId = CurrentUserId,
            Title = dto.Title,
            Description = dto.Description
        };

        await taskRepository.AddAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Обновляет существующую задачу.
    /// </summary>
    /// <param name="id">Идентификатор обновляемой задачи.</param>
    /// <param name="dto">Новые данные для задачи.</param>
    /// <returns>Статус NoContent в случае успешного обновления.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var task = await taskRepository.GetByIdAndUserAsync(id, CurrentUserId);
        if (task is null) return NotFound();

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task);
        return NoContent();
    }

    /// <summary>
    /// Удаляет задачу по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор удаляемой задачи.</param>
    /// <returns>Статус NoContent в случае успешного удаления.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await taskRepository.GetByIdAndUserAsync(id, CurrentUserId);
        if (task is null) return NotFound();

        await taskRepository.DeleteAsync(task);
        return NoContent();
    }
}
