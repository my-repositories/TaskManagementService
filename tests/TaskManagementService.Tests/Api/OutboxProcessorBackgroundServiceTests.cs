using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using TaskManagementService.Api.BackgroundServices;
using TaskManagementService.Dal;
using TaskManagementService.Domain.Interfaces;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Tests.Api;

public class OutboxProcessorBackgroundServiceTests
{
    private readonly AppDbContext _context;
    private readonly IServiceScopeFactory _scopeFactoryMock;
    private readonly ITaskEventService _eventServiceMock;
    private readonly ILogger<OutboxProcessorBackgroundService> _loggerMock;

    public OutboxProcessorBackgroundServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        _eventServiceMock = Substitute.For<ITaskEventService>();
        _loggerMock = Substitute.For<ILogger<OutboxProcessorBackgroundService>>();

        var scopeMock = Substitute.For<IServiceScope>();
        var serviceProviderMock = Substitute.For<IServiceProvider>();

        _scopeFactoryMock.CreateScope().Returns(scopeMock);
        scopeMock.ServiceProvider.Returns(serviceProviderMock);
        
        serviceProviderMock.GetService(typeof(AppDbContext)).Returns(_context);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProcessPendingMessages_AndMarkAsProcessed()
    {
        var task = new TaskItem { Id = 1, UserId = "1", Title = "Outbox Тест" };
        var pendingMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Action = "Created",
            Payload = JsonSerializer.Serialize(task),
            OccurredOn = DateTime.UtcNow
        };
        
        _context.OutboxMessages.Add(pendingMessage);
        await _context.SaveChangesAsync();

        var processor = new OutboxProcessorBackgroundService(_scopeFactoryMock, _eventServiceMock, _loggerMock);
        using var cts = new CancellationTokenSource();

        var startTime = DateTime.UtcNow;
        var runningTask = processor.StartAsync(cts.Token);

        await Task.Delay(150);
        await cts.CancelAsync();
        try { await runningTask; } catch (OperationCanceledException) { }

        await _eventServiceMock.Received(1).NotifyAsync("Created", Arg.Is<TaskItem>(t => t.Title == "Outbox Тест"));

        var processedMessage = await _context.OutboxMessages.FirstAsync();
        processedMessage.ProcessedOn.Should().NotBeNull();
        processedMessage.ProcessedOn.Should().BeAfter(startTime);
        processedMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotificationFails_ShouldSaveErrorToMessage()
    {
        var task = new TaskItem { Id = 2, UserId = "1", Title = "Упавший Тест" };
        var pendingMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Action = "Updated",
            Payload = JsonSerializer.Serialize(task),
            OccurredOn = DateTime.UtcNow
        };
        
        _context.OutboxMessages.Add(pendingMessage);
        await _context.SaveChangesAsync();

        _eventServiceMock.NotifyAsync(Arg.Any<string>(), Arg.Any<TaskItem>())
            .Throws(new Exception("Сбой сети RabbitMQ / HTTP"));

        var processor = new OutboxProcessorBackgroundService(_scopeFactoryMock, _eventServiceMock, _loggerMock);
        using var cts = new CancellationTokenSource();

        var runningTask = processor.StartAsync(cts.Token);
        await Task.Delay(150);
        await cts.CancelAsync();
        try { await runningTask; } catch (OperationCanceledException) { }

        var processedMessage = await _context.OutboxMessages.FirstAsync();
        processedMessage.ProcessedOn.Should().BeNull();
        processedMessage.Error.Should().Be("Сбой сети RabbitMQ / HTTP");
    }
}
