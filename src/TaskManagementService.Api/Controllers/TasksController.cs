using Microsoft.AspNetCore.Mvc;
using TaskManagementService.Domain.Dto;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(ITaskRepository taskRepository) : ControllerBase
{
    private string CurrentUserId => "1";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll()
    {
        var tasks = await taskRepository.GetAllByUserIdAsync(CurrentUserId);
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await taskRepository.GetByIdAndUserAsync(id, CurrentUserId);
        if (task is null) return NotFound();
        return Ok(task);
    }

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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await taskRepository.GetByIdAndUserAsync(id, CurrentUserId);
        if (task is null) return NotFound();

        await taskRepository.DeleteAsync(task);
        return NoContent();
    }
}
