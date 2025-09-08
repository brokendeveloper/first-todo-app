using Microsoft.EntityFrameworkCore;
using MyTodo.Models;

namespace MyTodo.Data;

public class AppDbContext: DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Todo> Todos { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>()
            .Property(b => b.Date)
            .HasDefaultValueSql("now() at time zone 'utc'"); // Específico para PostgreSQL
    }
    
}