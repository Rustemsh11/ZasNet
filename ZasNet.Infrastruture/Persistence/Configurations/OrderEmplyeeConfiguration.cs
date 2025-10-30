using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class OrderEmplyeeConfiguration : IEntityTypeConfiguration<OrderEmployee>
{
    public void Configure(EntityTypeBuilder<OrderEmployee> builder)
    {
        builder.ToTable("OrderEmployees");

        builder.HasOne(c => c.Order).WithMany(c => c.OrderEmployees).HasForeignKey(c => c.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Employee).WithMany(c => c.OrderEmployees).HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Restrict);

    }
}
