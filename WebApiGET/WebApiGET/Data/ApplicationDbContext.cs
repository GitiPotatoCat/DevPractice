
using Microsoft.EntityFrameworkCore;
using WebApiGET.Models;

namespace WebApiGET.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) 
    { }

    public DbSet<Owner> Owners { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
    }
}