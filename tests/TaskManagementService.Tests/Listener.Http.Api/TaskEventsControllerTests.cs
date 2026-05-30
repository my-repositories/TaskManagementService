using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskManagementService.Domain.Models;
using TaskManagementService.Listener.Http.Api.Controllers;

namespace TaskManagementService.Tests.Api;

public class TaskEventsControllerTests
{
    private readonly ILogger<TaskEventsController> _loggerMock;
    private readonly TaskEventsController _controller;

    public TaskEventsControllerTests()
    {
        _loggerMock = Substitute.For<ILogger<TaskEventsController>>();
        _controller = new TaskEventsController(_loggerMock);
    }

    [Fact]
    public void HandleEvent_ShouldReturnOkResult_WhenValidRequestReceived()
    {
        var action = "Created";
        var task = new TaskItem { Id = 1, UserId = "1", Title = "Sync Http Task" };

        var result = _controller.HandleEvent(action, task);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public void HandleEvent_ShouldLogInformationMessage_WhenValidRequestReceived()
    {
        var action = "Updated";
        var task = new TaskItem { Id = 2, UserId = "1", Title = "Log Test Task" };

        _controller.HandleEvent(action, task);

        _loggerMock.ReceivedWithAnyArgs().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
