using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SysPro.Domain.Entities;

namespace SysPro.DB.Persistence.Config;

public class ImportAuditConfiguration : IEntityTypeConfiguration<ImportAudit>
{
    public void Configure(EntityTypeBuilder<ImportAudit> builder)
    {
        builder.ToTable("ImportAudits", "App");
        
        builder.HasKey(i => i.ImportId);
        
        builder.Property(i => i.ImportId)
            .HasDefaultValueSql("newsequentialid()")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.FileName)
            .IsRequired();
    }
}