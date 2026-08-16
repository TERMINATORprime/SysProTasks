using Microsoft.EntityFrameworkCore;
using SysPro.Domain.Entities;

namespace SysPro.DB.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Orders>  Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<OrderVersion> OrderVersions { get; set; }
    public DbSet<ImportAudit> ImportAudits { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
        => builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}