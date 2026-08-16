using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SysPro.Domain.Entities;

namespace SysPro.DB.Persistence.Config;

public class OrderVersionConfiguration : IEntityTypeConfiguration<OrderVersion>
{
    public void Configure(EntityTypeBuilder<OrderVersion> builder)
    {
        builder.ToTable("OrderVersion", "Orders");

        builder.HasKey(o => o.OrderVersionId);
        
        builder.Property(o => o.OrderVersionId)
            .HasDefaultValueSql("newsequentialid()")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.VersionNumber)
            .IsRequired();
        
        builder.HasOne<Orders>()
            .WithMany()
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}