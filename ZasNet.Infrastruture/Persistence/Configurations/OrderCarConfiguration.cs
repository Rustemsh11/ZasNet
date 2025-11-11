using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class OrderCarConfiguration : IEntityTypeConfiguration<OrderServiceCar>
{
    public void Configure(EntityTypeBuilder<OrderServiceCar> builder)
    {
        builder.ToTable("OrderServiceCars");

        builder.HasOne(c => c.OrderService).WithMany(c => c.OrderServiceCars).HasForeignKey(c => c.OrderServiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Car).WithMany(c => c.OrderServiceCars).HasForeignKey(c => c.CarId).OnDelete(DeleteBehavior.Restrict);
    }
}
