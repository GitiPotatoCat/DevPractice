using FootballApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballApp.Api.Data;

public class FootballDbContext : DbContext
{
    public FootballDbContext(DbContextOptions<FootballDbContext> options)
        : base(options)
    {
    }

    public DbSet<FootballClub> FootballClubs => Set<FootballClub>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FootballClub>(entity =>
        {
            // Map to the exact table created in Step 1.
            entity.ToTable("FootballClubs", schema: "dbo");

            entity.HasKey(c => c.Id);

            // Identity column - DB generates the value on insert.
            entity.Property(c => c.Id).ValueGeneratedOnAdd();

            entity.Property(c => c.ClubName)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(c => c.Country)
                  .IsRequired()
                  .HasMaxLength(80);

            entity.Property(c => c.League)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.Stadium)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(c => c.FoundedYear).IsRequired();
            entity.Property(c => c.TitlesWon).IsRequired();
        });
    }
}