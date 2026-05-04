using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Streak> Streaks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        // Unique values per table / model
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<Streak>()
            .HasIndex(s => s.UserId)
            .IsUnique();
        
        // Enums saved in the database as strings
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Type)
            .HasConversion<string>();
        
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Priority)
            .HasConversion<string>();
    }
}