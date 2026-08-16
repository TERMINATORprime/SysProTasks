using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SysPro.Domain.Entities;

namespace SysPro.DB.Persistence.Config;

public class OrderConfiguration : IEntityTypeConfiguration<Orders>
{
    public void Configure(EntityTypeBuilder<Orders> builder)
    {
        builder.ToTable("Orders", "Orders");
        
        builder.HasKey(o => o.OrderId);
        
        builder.Property(o => o.OrderId)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .ValueGeneratedOnAdd();
        
        builder.Property(o => o.OrderExternalID)
            .IsRequired()
            .HasMaxLength(250);
        
        builder.HasIndex(o => o.OrderExternalID)
            .IsUnique();

        builder.Property(o => o.OrderDate)
            .HasColumnType("date");

        builder.Property(o => o.CreatedDate)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();
        
        builder.Property(ol => ol.ModifiedDate)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();
    }
}