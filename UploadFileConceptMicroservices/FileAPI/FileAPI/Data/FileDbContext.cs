

using FileAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FileAPI.Data;


public class FileDbContext : DbContext 
{
    public FileDbContext(DbContextOptions<FileDbContext> options) 
        : base(options) { }

    public DbSet<FileRecord> Files => Set<FileRecord>();


    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        var e = modelBuilder.Entity<FileRecord>();

        e.Property<byte[]>("Data")
            .HasColumnType("varbinary(max)")
            .IsRequired();

        e.Property(p => p.OriginalName)
            .HasMaxLength(255)
            .IsRequired();

        e.Property(p => p.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        e.Property(p => p.Size)
            .IsRequired();

        e.Property(p => p.Sha256)
            .HasMaxLength(64)
            .IsRequired();

        e.Property(p => p.CreatedUtc)
            .IsRequired();
    }
}