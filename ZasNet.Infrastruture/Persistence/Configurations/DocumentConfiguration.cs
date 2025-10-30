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
        builder.Property(c=>c.Path).IsRequired();
        builder.Property(c=>c.UploadedDate).IsRequired().HasDefaultValueSql("GETDATE()");
        builder.Property(c => c.DocumentType).IsRequired().HasConversion(documentTypeConverter);

        builder.HasOne(c=>c.User).WithMany().HasForeignKey(c=>c.UploadedUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c=>c.Order).WithMany(c=>c.OrderDocuments).HasForeignKey(c=>c.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
