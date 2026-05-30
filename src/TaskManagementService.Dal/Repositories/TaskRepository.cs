using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Dal.Repositories;

public class TaskRepository(AppDbContext context) : ITaskRepository
{
    public async Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(string userId) =>
        await context.Tasks.Where(t => t.UserId == userId).ToListAsync();

    public async Task<TaskItem?> GetByIdAndUserAsync(int id, string userId) =>
        await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task AddAsync(TaskItem task)
    {
        context.Tasks.Add(task);
        EnqueueOutbox("Created", task);

        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        context.Tasks.Update(task);
        EnqueueOutbox("Updated", task);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        context.Tasks.Remove(task);
        EnqueueOutbox("Deleted", task);
        await context.SaveChangesAsync();
    }

    private void EnqueueOutbox(string action, TaskItem task)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Action = action,
            Payload = JsonSerializer.Serialize(task),
            OccurredOn = DateTime.UtcNow
        };
        context.OutboxMessages.Add(outboxMessage);
    }
}
