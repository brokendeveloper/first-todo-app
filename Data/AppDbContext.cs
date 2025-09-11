using Microsoft.EntityFrameworkCore;
using MyTodo.Models;

namespace MyTodo.Data;

public class AppDbContext : DbContext
{
    public DbSet<Todo> Todos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CustomLog> CustomLogs { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração CustomLog
        modelBuilder.Entity<CustomLog>(entity =>
        {
            entity.ToTable("custom_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.DescriptionJson)
                .HasColumnType("jsonb")
                .IsRequired(); 

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()")
                .IsRequired();

            
            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired(false);

            entity.Property(e => e.ActionId)
                .HasColumnName("action_id")
                .IsRequired();

            
        });

        // Configurações para User (opcional, mas bom deixar claro)
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150);

            // Relação 1:N User -> Todos, sem FK obrigatória na tabela Todo
            entity.HasMany(u => u.Todos)
                .WithOne() // Sem propriedade de navegação do Todo para User
                .HasForeignKey(t => t.UserId) // FK real em Todo
                .IsRequired();
        });

        // Configurações para Todo
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.ToTable("todos");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(e => e.Done)
                .IsRequired();

            entity.Property(e => e.DueDate)
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            
            
        });

        base.OnModelCreating(modelBuilder);
    }
}
