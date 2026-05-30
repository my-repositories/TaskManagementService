using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskManagementService.Domain.Models;
using TaskManagementService.Listener.Rabbit.Api.Handlers;
using TaskStatus = TaskManagementService.Domain.Enums.TaskStatus;

namespace TaskManagementService.Tests.Api;

public class TaskCreatedEventHandlerTests
{
    private readonly ILogger<TaskCreatedEventHandler> _loggerMock;
    private readonly TaskCreatedEventHandler _handler;

    public TaskCreatedEventHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<TaskCreatedEventHandler>>();
        _handler = new TaskCreatedEventHandler(_loggerMock);
    }

    [Fact]
    public void ActionName_ShouldMatchCreatedString()
    {
        _handler.ActionName.Should().Be("Created");
    }

    [Fact]
    public async Task HandleAsync_ShouldLogInformationMessage_WhenTaskProvided()
    {
        var task = new TaskItem
        {
            Id = 10,
            UserId = "1",
            Title = "Rabbit Handler Task",
            Status = TaskStatus.New
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
