using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        var documentTypeConverter = new EnumToNumberConverter<DocumentType, short>();
        builder.ToTable("Documents");

        builder.Property(c=>c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Extension).IsRequired().HasMaxLength(15);
        builder.Property(c=>c.Path).IsRequired(false);
        builder.Property(c => c.Content).HasColumnType("bytea").IsRequired(false);
        builder.Property(c => c.ContentType).HasMaxLength(100).IsRequired(false);
        builder.Property(c => c.SizeBytes).IsRequired(false);
        builder.Property(c=>c.UploadedDate).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(c => c.DocumentType).IsRequired().HasConversion(documentTypeConverter);

        builder.HasOne(c=>c.Order).WithMany(c=>c.OrderDocuments).HasForeignKey(c=>c.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
