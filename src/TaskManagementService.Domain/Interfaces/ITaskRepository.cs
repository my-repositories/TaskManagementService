using TaskManagementService.Domain.Models;

namespace TaskManagementService.Domain.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(string userId);
    Task<TaskItem?> GetByIdAndUserAsync(int id, string userId);
    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}
