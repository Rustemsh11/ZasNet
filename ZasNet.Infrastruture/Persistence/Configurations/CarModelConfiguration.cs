using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class CarModelConfiguration : IEntityTypeConfiguration<CarModel>
{
    public void Configure(EntityTypeBuilder<CarModel> builder)
    {
        builder.ToTable("CarModels");

        builder.Property(c=>c.Name).IsRequired().HasMaxLength(150);
    }
}
