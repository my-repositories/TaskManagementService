using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TaskManagementService.Domain.Configurations;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;
using TaskManagementService.Listener.Rabbit.Api.BackgroundServices;

namespace TaskManagementService.Tests.Api;

public class RabbitMqConsumerServiceTests
{
    private readonly ILogger<RabbitMqConsumerService> _loggerMock;
    private readonly IOptions<RabbitMqOptions> _optionsMock;
    private readonly IServiceScopeFactory _scopeFactoryMock;
    private readonly IServiceScope _scopeMock;
    private readonly IServiceProvider _serviceProviderMock;

    public RabbitMqConsumerServiceTests()
    {
        _loggerMock = Substitute.For<ILogger<RabbitMqConsumerService>>();
        _optionsMock = Substitute.For<IOptions<RabbitMqOptions>>();
        _scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        _scopeMock = Substitute.For<IServiceScope>();
        _serviceProviderMock = Substitute.For<IServiceProvider>();

        _optionsMock.Value.Returns(new RabbitMqOptions
        {
            Host = "localhost",
            QueueName = "test-queue",
            UserName = "guest",
            Password = "guest"
        });

        _scopeFactoryMock.CreateScope().Returns(_scopeMock);
        _scopeMock.ServiceProvider.Returns(_serviceProviderMock);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenValidDependenciesProvided()
    {
        var service = new RabbitMqConsumerService(_loggerMock, _optionsMock, _scopeFactoryMock);

        service.Should().NotBeNull();
    }

    [Fact]
    public async Task Dispatcher_ShouldResolveHandlersFromScope_AndExecuteCorrectStrategy()
    {
        var handlerMock = Substitute.For<ITaskEventHandler>();
        handlerMock.ActionName.Returns("Created");

        var handlersList = new List<ITaskEventHandler> { handlerMock };
        _serviceProviderMock.GetService(typeof(IEnumerable<ITaskEventHandler>)).Returns(handlersList);

        using var scope = _scopeFactoryMock.CreateScope();
        var resolvedHandlers = scope.ServiceProvider.GetServices<ITaskEventHandler>();
        var targetHandler = resolvedHandlers.FirstOrDefault(h => h.ActionName.Equals("Created", StringComparison.OrdinalIgnoreCase));

        targetHandler.Should().NotBeNull();
        targetHandler!.ActionName.Should().Be("Created");

        var task = new TaskItem { Id = 1, UserId = "1", Title = "Rabbit Strategy Task" };
        await targetHandler.HandleAsync(task, CancellationToken.None);

        await handlerMock.Received(1).HandleAsync(task, CancellationToken.None);
    }

    [Fact]
    public void Dispatcher_ShouldLogWarning_WhenHandlerForActionNotFound()
    {
        var handlersList = new List<ITaskEventHandler>();
        _serviceProviderMock.GetService(typeof(IEnumerable<ITaskEventHandler>)).Returns(handlersList);

        using var scope = _scopeFactoryMock.CreateScope();
        var resolvedHandlers = scope.ServiceProvider.GetServices<ITaskEventHandler>();
        var targetHandler = resolvedHandlers.FirstOrDefault(h => h.ActionName.Equals("UnknownAction", StringComparison.OrdinalIgnoreCase));

        targetHandler.Should().BeNull();
        
        _loggerMock.LogWarning("[RabbitMqConsumerService] Не найден обработчик для события: UnknownAction");

        _loggerMock.ReceivedWithAnyArgs().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
