using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SysPro.Domain.Entities;

namespace SysPro.DB.Persistence.Config;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines", "Orders");
        
        builder.HasKey(ol => ol.OrderLineId);
        
        builder.Property(ol => ol.OrderLineId)
            .HasDefaultValueSql("newsequentialid()")
            .ValueGeneratedOnAdd();
        
        builder.Property(ol => ol.Sku)
            .IsRequired()
            .HasMaxLength(64);
        
        builder.Property(ol => ol.Currency)
            .HasMaxLength(3)
            .IsFixedLength();
        
        builder.Property(ol => ol.CreateDate)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();
        
        builder.Property(ol => ol.UpdateDate)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();
        
        builder.HasOne<Orders>()
            .WithMany()
            .HasForeignKey(ol => ol.OrderID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}