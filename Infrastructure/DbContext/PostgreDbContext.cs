using Domain.Entities.Implementations;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DbContext;

public class PostgreDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<GameTheme> GameThemes { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<AiRecord> AiRecords { get; set; }

    public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        BuildUserModel(modelBuilder);
        modelBuilder.Entity<GameTheme>().HasData(
            [
                new GameTheme("Фэнтези", "Fantasy") { Id = Guid.NewGuid() }
            ]
        );
    }

    private static void BuildUserModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(e => e.Status)
            .HasConversion(s => s.ToString(),
                s => (Statuses)Enum.Parse(typeof(Statuses), s));
        modelBuilder.Entity<Game>()
            .Property(e => e.GameState)
            .HasConversion(s => s.ToString(),
                s => (GameState)Enum.Parse(typeof(GameState), s));
    }
}