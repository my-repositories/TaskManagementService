using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskManagementService.Domain.Models;
using TaskManagementService.Listener.Rabbit.Api.Handlers;
using TaskStatus = TaskManagementService.Domain.Enums.TaskStatus;

namespace TaskManagementService.Tests.Api;

public class TaskUpdatedEventHandlerTests
{
    private readonly ILogger<TaskUpdatedEventHandler> _loggerMock;
    private readonly TaskUpdatedEventHandler _handler;

    public TaskUpdatedEventHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<TaskUpdatedEventHandler>>();
        _handler = new TaskUpdatedEventHandler(_loggerMock);
    }

    [Fact]
    public void ActionName_ShouldMatchUpdatedString()
    {
        _handler.ActionName.Should().Be("Updated");
    }

    [Fact]
    public async Task HandleAsync_ShouldLogInformationMessage_WhenTaskProvided()
    {
        var task = new TaskItem
        {
            Id = 30,
            UserId = "1",
            Title = "Updated Handler Task",
            Status = TaskStatus.InProgress
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
