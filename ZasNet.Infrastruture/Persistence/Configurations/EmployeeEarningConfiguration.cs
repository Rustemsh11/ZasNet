using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class EmployeeEarningConfiguration : IEntityTypeConfiguration<EmployeeEarinig>
{
    public void Configure(EntityTypeBuilder<EmployeeEarinig> builder)
    {
        builder.ToTable("EmployeeEarnings");

        builder.Property(c => c.ServiceEmployeePrecent).IsRequired();
        builder.Property(c => c.PrecentEmployeeDescription).HasMaxLength(500);
        builder.Property(c => c.EmployeeEarning).IsRequired();

        builder.HasOne(c => c.OrderService)
            .WithOne(c=>c.EmployeeEarinig)
            .HasForeignKey<EmployeeEarinig>(c=>c.OrderServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

