using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class OrderEmplyeeConfiguration : IEntityTypeConfiguration<OrderServiceEmployee>
{
    public void Configure(EntityTypeBuilder<OrderServiceEmployee> builder)
    {
        builder.ToTable("OrderServiceEmployees");

        builder.HasOne(c => c.OrderService).WithMany(c => c.OrderServiceEmployees).HasForeignKey(c => c.OrderServiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Employee).WithMany(c => c.OrderServiceEmployees).HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Restrict);

    }
}
