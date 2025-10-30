using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class OrderServiceConfiguration : IEntityTypeConfiguration<OrderService>
{
    public void Configure(EntityTypeBuilder<OrderService> builder)
    {
        builder.ToTable("OrderServices");

        builder.Property(c=>c.Price).IsRequired();
        builder.Property(c=>c.TotalVolume).IsRequired();
        builder.Property(c=>c.PriceTotal).IsRequired();

        builder.HasOne(c=>c.Order).WithMany(c=>c.OrderServices).HasForeignKey(c=>c.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c=>c.Service).WithMany(c=>c.OrderServices).HasForeignKey(c=>c.ServiceId).OnDelete(DeleteBehavior.Restrict);

    }
}
