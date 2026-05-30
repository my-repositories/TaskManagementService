using Microsoft.EntityFrameworkCore;
using TaskManagementService.Domain.Models;

namespace TaskManagementService.Dal;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasIndex(t => t.UserId);
            entity.Property(t => t.Title).HasMaxLength(255);
        });
    }
}
