using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskManagementService.Domain.Models;
using TaskManagementService.Listener.Rabbit.Api.Handlers;
using TaskStatus = TaskManagementService.Domain.Enums.TaskStatus;

namespace TaskManagementService.Tests.Api;

public class TaskDeletedEventHandlerTests
{
    private readonly ILogger<TaskDeletedEventHandler> _loggerMock;
    private readonly TaskDeletedEventHandler _handler;

    public TaskDeletedEventHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<TaskDeletedEventHandler>>();
        _handler = new TaskDeletedEventHandler(_loggerMock);
    }

    [Fact]
    public void ActionName_ShouldMatchDeletedString()
    {
        _handler.ActionName.Should().Be("Deleted");
    }

    [Fact]
    public async Task HandleAsync_ShouldLogInformationMessage_WhenTaskProvided()
    {
        var task = new TaskItem 
        { 
            Id = 20, 
            UserId = "1", 
            Title = "Deleted Handler Task", 
            Status = TaskStatus.Completed 
        };

        await _handler.HandleAsync(task, CancellationToken.None);

        _loggerMock.ReceivedWithAnyArgs().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
