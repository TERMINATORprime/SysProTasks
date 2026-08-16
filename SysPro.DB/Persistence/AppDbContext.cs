using Microsoft.EntityFrameworkCore;
using SysPro.Domain.Entities;
using SysPro.Domain.Models;

namespace SysPro.DB.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<OrderVersion> OrderVersions { get; set; }
    public DbSet<ImportAudit> ImportAudits { get; set; }
    
    public DbSet<OrdersViewModel> OrdersView => Set<OrdersViewModel>();
    public DbSet<SummaryViewModel> SummaryView => Set<SummaryViewModel>();
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        builder.Entity<OrdersViewModel>(e =>
        {
            e.HasNoKey();
            e.ToView(null);
        });

        builder.Entity<SummaryViewModel>(e =>
        {
            e.HasNoKey();
            e.ToView(null);
        });
        
        base.OnModelCreating(builder);
    }
}