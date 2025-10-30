using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class OrderCarConfiguration : IEntityTypeConfiguration<OrderCar>
{
    public void Configure(EntityTypeBuilder<OrderCar> builder)
    {
        builder.ToTable("OrderCars");

        builder.HasOne(c => c.Order).WithMany(c => c.OrderCars).HasForeignKey(c => c.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Car).WithMany(c => c.OrderCars).HasForeignKey(c => c.CarId).OnDelete(DeleteBehavior.Restrict);
    }
}
