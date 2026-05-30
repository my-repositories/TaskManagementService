using Microsoft.EntityFrameworkCore;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Dal;

/// <summary>
/// Контекст базы данных для управления задачами и сообщениями Outbox.
/// </summary>
/// <param name="options">Параметры конфигурации контекста.</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Таблица задач.
    /// </summary>
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    /// <summary>
    /// Таблица исходящих интеграционных сообщений.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Конфигурация ограничений моделей и индексов базы данных.
    /// </summary>
    /// <param name="modelBuilder">Построитель моделей данных.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasIndex(t => t.UserId);
            entity.Property(t => t.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasIndex(o => o.ProcessedOn);
        });
    }
}
