using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TaskManagementService.Api.Services;
using TaskManagementService.Domain.Configurations;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Tests.Api;

public class TaskEventServiceTests
{
    private readonly IConfiguration _configurationMock;
    private readonly ILogger<TaskEventService> _loggerMock;
    private readonly RabbitMqOptions _rabbitOptions;
    private readonly IOptions<RabbitMqOptions> _rabbitOptionsMock;
    private readonly FakeHttpMessageHandler _httpHandler;
    private readonly HttpClient _httpClient;

    public TaskEventServiceTests()
    {
        _configurationMock = Substitute.For<IConfiguration>();
        _loggerMock = Substitute.For<ILogger<TaskEventService>>();
        _rabbitOptionsMock = Substitute.For<IOptions<RabbitMqOptions>>();

        _configurationMock["ExternalServices:HttpListenerUrl"].Returns("http://localhost:5002/api/task-events");

        _rabbitOptions = new RabbitMqOptions
        {
            Host = "localhost",
            QueueName = "test-queue",
            UserName = "guest",
            Password = "guest"
        };
        _rabbitOptionsMock.Value.Returns(_rabbitOptions);

        _httpHandler = new FakeHttpMessageHandler();
        _httpClient = new HttpClient(_httpHandler);
    }

    [Fact]
    public async Task NotifyHttpListenerAsync_ShouldSendPostRequest_AndSucceed()
    {
        _httpHandler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK);
        var service = new TaskEventService(_httpClient, _configurationMock, _loggerMock, _rabbitOptionsMock);
        var task = new TaskItem { Id = 1, UserId = "1", Title = "Http Test" };

        await service.NotifyHttpListenerAsync("Created", task);

        _httpHandler.LastRequest.Should().NotBeNull();
        _httpHandler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        _httpHandler.LastRequest.RequestUri!.ToString().Should().Contain("action=Created");

        var content = await _httpHandler.LastRequest.Content!.ReadAsStringAsync();
        content.Should().Contain("Http Test");
    }

    [Fact]
    public async Task NotifyHttpListenerAsync_WhenHttpFails_ShouldLogWarningOrError_AndNotThrow()
    {
        _httpHandler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var service = new TaskEventService(_httpClient, _configurationMock, _loggerMock, _rabbitOptionsMock);
        var task = new TaskItem { Id = 1, UserId = "1", Title = "Http Fail Test" };

        var act = async () => await service.NotifyHttpListenerAsync("Created", task);

        await act.Should().NotThrowAsync();
        _loggerMock.ReceivedWithAnyArgs().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task NotifyMqListenerAsync_WhenBrokerUnreachable_ShouldLogException_AndNotThrow()
    {
        _rabbitOptions.Host = "non-existent-host-12345";
        _rabbitOptionsMock.Value.Returns(_rabbitOptions);
        var service = new TaskEventService(_httpClient, _configurationMock, _loggerMock, _rabbitOptionsMock);
        var task = new TaskItem { Id = 2, UserId = "1", Title = "Rabbit Fail Test" };

        var act = async () => await service.NotifyMqListenerAsync("Updated", task);

        await act.Should().NotThrowAsync();
        _loggerMock.ReceivedWithAnyArgs().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public HttpResponseMessage ResponseToReturn { get; set; } = new(HttpStatusCode.OK);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(ResponseToReturn);
        }
    }
}
