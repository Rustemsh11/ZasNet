using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.Property(c=>c.Name).IsRequired();
        builder.Property(c=>c.Price).IsRequired();
        builder.Property(c=>c.Measure).IsRequired().HasMaxLength(100);
        builder.Property(c=>c.MinVolume).IsRequired();
    }
}
