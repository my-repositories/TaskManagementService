using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskManagementService.Api.BackgroundServices;
using TaskManagementService.Dal;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Tests.Api;

public class OutboxProcessorBackgroundServiceTests
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private readonly IServiceScopeFactory _scopeFactoryMock;
    private readonly ITaskEventService _eventServiceMock;
    private readonly ILogger<OutboxProcessorBackgroundService> _loggerMock;
    private readonly ManualResetEventSlim _processingSignal;

    public OutboxProcessorBackgroundServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        _eventServiceMock = Substitute.For<ITaskEventService>();
        _loggerMock = Substitute.For<ILogger<OutboxProcessorBackgroundService>>();
        _processingSignal = new ManualResetEventSlim(false);

        var scopeMock = Substitute.For<IServiceScope>();
        var serviceProviderMock = Substitute.For<IServiceProvider>();

        _scopeFactoryMock.CreateScope().Returns(scopeMock);
        scopeMock.ServiceProvider.Returns(serviceProviderMock);

        serviceProviderMock.GetService(typeof(AppDbContext))
            .Returns(_ => new AppDbContext(_dbOptions));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessPendingMessages_AndMarkAsProcessed()
    {
        using (var context = new AppDbContext(_dbOptions))
        {
            var task = new TaskItem { Id = 1, UserId = "1", Title = "Outbox Тест" };
            var pendingMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Action = "Created",
                Payload = JsonSerializer.Serialize(task),
                OccurredOn = DateTime.UtcNow
            };
            context.OutboxMessages.Add(pendingMessage);
            await context.SaveChangesAsync();
        }

        _eventServiceMock.NotifyAsync(Arg.Any<string>(), Arg.Any<TaskItem>())
            .Returns(_ =>
            {
                _processingSignal.Set();
                return Task.CompletedTask;
            });

        var processor = new OutboxProcessorBackgroundService(_scopeFactoryMock, _eventServiceMock, _loggerMock);
        using var cts = new CancellationTokenSource();
        var startTime = DateTime.UtcNow;

        await processor.StartAsync(cts.Token);
        _processingSignal.Wait(2000);
        await cts.CancelAsync();

        await _eventServiceMock.Received(1).NotifyAsync("Created", Arg.Is<TaskItem>(t => t.Title == "Outbox Тест"));

        using (var context = new AppDbContext(_dbOptions))
        {
            var processedMessage = await context.OutboxMessages.FirstAsync();
            processedMessage.ProcessedOn.Should().NotBeNull();
            processedMessage.ProcessedOn.Should().BeAfter(startTime);
            processedMessage.Error.Should().BeNull();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotificationFails_ShouldSaveErrorToMessage()
    {
        using (var context = new AppDbContext(_dbOptions))
        {
            var task = new TaskItem { Id = 2, UserId = "1", Title = "Упавший Тест" };
            var pendingMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Action = "Updated",
                Payload = JsonSerializer.Serialize(task),
                OccurredOn = DateTime.UtcNow
            };
            context.OutboxMessages.Add(pendingMessage);
            await context.SaveChangesAsync();
        }

        _eventServiceMock.NotifyAsync(Arg.Any<string>(), Arg.Any<TaskItem>())
            .Throws(_ =>
            {
                _processingSignal.Set();
                return new Exception("Сбой сети RabbitMQ / HTTP");
            });

        var processor = new OutboxProcessorBackgroundService(_scopeFactoryMock, _eventServiceMock, _loggerMock);
        using var cts = new CancellationTokenSource();

        await processor.StartAsync(cts.Token);
        _processingSignal.Wait(2000);
        await cts.CancelAsync();

        using (var context = new AppDbContext(_dbOptions))
        {
            var processedMessage = await context.OutboxMessages.FirstAsync();
            processedMessage.Error.Should().Be("Сбой сети RabbitMQ / HTTP");
            processedMessage.ProcessedOn.Should().BeNull();
        }
    }
}
