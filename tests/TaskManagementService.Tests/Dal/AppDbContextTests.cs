using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagementService.Dal;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Tests.Dal;

public class AppDbContextTests
{
    private readonly AppDbContext _context;

    public AppDbContextTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public void AppDbContext_ShouldContainRequiredDbSets()
    {
        _context.Tasks.Should().NotBeNull();
        _context.OutboxMessages.Should().NotBeNull();
    }

    [Fact]
    public void ModelBuilder_ShouldApplyTitleMaxLengthConstraintToTaskItem()
    {
        var entityType = _context.Model.FindEntityType(typeof(TaskItem));
        var titleProperty = entityType?.FindProperty(nameof(TaskItem.Title));

        titleProperty.Should().NotBeNull();
        titleProperty!.GetMaxLength().Should().Be(255);
    }

    [Fact]
    public void ModelBuilder_ShouldCreateUserIdIndexOnTaskItem()
    {
        var entityType = _context.Model.FindEntityType(typeof(TaskItem));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(TaskItem.UserId)));

        index.Should().NotBeNull();
    }

    [Fact]
    public void ModelBuilder_ShouldCreateProcessedOnIndexOnOutboxMessage()
    {
        var entityType = _context.Model.FindEntityType(typeof(OutboxMessage));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(OutboxMessage.ProcessedOn)));

        index.Should().NotBeNull();
    }
}
