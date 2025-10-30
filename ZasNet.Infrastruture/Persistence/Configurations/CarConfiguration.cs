using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        var carStatusConvertion = new EnumToNumberConverter<CarStatus, short>();

        builder.ToTable("Cars");

        builder.Property(c=>c.Number).IsRequired().HasMaxLength(15);
        builder.Property(c=>c.Status).IsRequired().HasConversion(carStatusConvertion);

        builder.HasOne(c=>c.CarModel).WithMany(c=>c.Cars).HasForeignKey(c=>c.CarModelId).OnDelete(DeleteBehavior.Restrict);
    }
}
