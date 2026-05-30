using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TaskManagementService.Api.Controllers;
using TaskManagementService.Domain.Dto;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;
using TaskStatus = TaskManagementService.Domain.Enums.TaskStatus;

namespace TaskManagementService.Tests.Api;

public class TasksControllerTests
{
    private readonly ITaskRepository _repositoryMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _repositoryMock = Substitute.For<ITaskRepository>();
        _controller = new TasksController(_repositoryMock);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithTasksForCurrentUser()
    {
        var expectedTasks = new List<TaskItem>
        {
            new() { Id = 1, UserId = "1", Title = "Task 1" },
            new() { Id = 2, UserId = "1", Title = "Task 2" }
        };
        _repositoryMock.GetAllByUserIdAsync("1").Returns(expectedTasks);

        var result = await _controller.GetAll();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        returnedTasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenTaskExists()
    {
        var expectedTask = new TaskItem { Id = 42, UserId = "1", Title = "Existing Task" };
        _repositoryMock.GetByIdAndUserAsync(42, "1").Returns(expectedTask);

        var result = await _controller.GetById(42);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Id.Should().Be(42);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        _repositoryMock.GetByIdAndUserAsync(99, "1").Returns((TaskItem?)null);

        var result = await _controller.GetById(99);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_AndCallRepository()
    {
        var dto = new CreateTaskDto { Title = "New Task", Description = "Desc" };

        var result = await _controller.Create(dto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TasksController.GetById));

        var returnedTask = createdResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Title.Should().Be("New Task");
        returnedTask.UserId.Should().Be("1");

        await _repositoryMock.Received(1).AddAsync(Arg.Is<TaskItem>(t => t.Title == "New Task" && t.UserId == "1"));
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenTaskExistsAndIsUpdated()
    {
        var existingTask = new TaskItem { Id = 10, UserId = "1", Title = "Old Title" };
        var dto = new UpdateTaskDto { Title = "New Title", Description = "New Desc", Status = TaskStatus.InProgress };
        _repositoryMock.GetByIdAndUserAsync(10, "1").Returns(existingTask);

        var result = await _controller.Update(10, dto);

        result.Should().BeOfType<NoContentResult>();
        existingTask.Title.Should().Be("New Title");
        existingTask.Description.Should().Be("New Desc");
        existingTask.Status.Should().Be(TaskStatus.InProgress);
        await _repositoryMock.Received(1).UpdateAsync(existingTask);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var dto = new UpdateTaskDto { Title = "Title", Status = TaskStatus.Completed };
        _repositoryMock.GetByIdAndUserAsync(10, "1").Returns((TaskItem?)null);

        var result = await _controller.Update(10, dto);

        result.Should().BeOfType<NotFoundResult>();
        await _repositoryMock.DidNotReceiveWithAnyArgs().UpdateAsync(Arg.Any<TaskItem>());
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenTaskExistsAndIsDeleted()
    {
        var existingTask = new TaskItem { Id = 5, UserId = "1", Title = "To Delete" };
        _repositoryMock.GetByIdAndUserAsync(5, "1").Returns(existingTask);

        var result = await _controller.Delete(5);

        result.Should().BeOfType<NoContentResult>();
        await _repositoryMock.Received(1).DeleteAsync(existingTask);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        _repositoryMock.GetByIdAndUserAsync(5, "1").Returns((TaskItem?)null);

        var result = await _controller.Delete(5);

        result.Should().BeOfType<NotFoundResult>();
        await _repositoryMock.DidNotReceiveWithAnyArgs().DeleteAsync(Arg.Any<TaskItem>());
    }
}
