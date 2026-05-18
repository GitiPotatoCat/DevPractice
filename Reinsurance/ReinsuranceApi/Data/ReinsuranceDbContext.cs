using Microsoft.EntityFrameworkCore;
using ReinsuranceApi.Models;

namespace ReinsuranceApi.Data;

public class ReinsuranceDbContext : DbContext
{
    public ReinsuranceDbContext(DbContextOptions<ReinsuranceDbContext> options) 
        : base(options) { }

    public DbSet<Treaty> Treaties => Set<Treaty>();
}