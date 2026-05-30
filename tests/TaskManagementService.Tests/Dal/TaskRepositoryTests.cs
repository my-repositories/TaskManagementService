using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagementService.Dal;
using TaskManagementService.Dal.Repositories;
using TaskManagementService.Domain.Models;
using TaskStatus = TaskManagementService.Domain.Enums.TaskStatus;

namespace TaskManagementService.Tests.Dal;

public class TaskRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new TaskRepository(_context);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnOnlyUserTasks()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, UserId = "1", Title = "User 1 Task 1" },
            new() { Id = 2, UserId = "1", Title = "User 1 Task 2" },
            new() { Id = 3, UserId = "2", Title = "User 2 Task" }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllByUserIdAsync("1");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.UserId == "1");
    }

    [Fact]
    public async Task GetByIdAndUserAsync_ShouldReturnTask_WhenIdAndUserMatch()
    {
        var task = new TaskItem { Id = 10, UserId = "1", Title = "Target Task" };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAndUserAsync(10, "1");

        result.Should().NotBeNull();
        result!.Title.Should().Be("Target Task");
    }

    [Fact]
    public async Task GetByIdAndUserAsync_ShouldReturnNull_WhenUserDoesNotMatch()
    {
        var task = new TaskItem { Id = 11, UserId = "2", Title = "Alien Task" };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAndUserAsync(11, "1");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldSaveTaskAndCreateOutboxMessage_Atomically()
    {
        var task = new TaskItem { Id = 100, UserId = "1", Title = "New Architecture Task" };

        await _repository.AddAsync(task);

        var savedTask = await _context.Tasks.FindAsync(100);
        savedTask.Should().NotBeNull();
        savedTask!.Title.Should().Be("New Architecture Task");

        var outboxMessage = await _context.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Action.Should().Be("Created");
        outboxMessage.Payload.Should().Contain("New Architecture Task");
        outboxMessage.ProcessedOn.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyTaskAndCreateOutboxMessage_Atomically()
    {
        var task = new TaskItem { Id = 200, UserId = "1", Title = "Old Title" };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _context.Entry(task).State = EntityState.Detached;

        var taskToUpdate = new TaskItem
        {
            Id = 200,
            UserId = "1",
            Title = "Updated Title",
            Status = TaskStatus.InProgress
        };

        await _repository.UpdateAsync(taskToUpdate);

        var updatedTask = await _context.Tasks.FindAsync(200);
        updatedTask!.Title.Should().Be("Updated Title");
        updatedTask.Status.Should().Be(TaskStatus.InProgress);

        var outboxMessage = await _context.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Action.Should().Be("Updated");
        outboxMessage.Payload.Should().Contain("Updated Title");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTaskAndCreateOutboxMessage_Atomically()
    {
        var task = new TaskItem { Id = 300, UserId = "1", Title = "To Be Deleted" };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(task);

        var deletedTask = await _context.Tasks.FindAsync(300);
        deletedTask.Should().BeNull();

        var outboxMessage = await _context.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Action.Should().Be("Deleted");
        outboxMessage.Payload.Should().Contain("To Be Deleted");
    }
}
