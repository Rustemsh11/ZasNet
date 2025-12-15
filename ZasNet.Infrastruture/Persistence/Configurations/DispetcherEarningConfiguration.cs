using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class DispetcherEarningConfiguration : IEntityTypeConfiguration<DispetcherEarning>
{
    public void Configure(EntityTypeBuilder<DispetcherEarning> builder)
    {
        builder.ToTable("DispetcherEarnings");

        builder.Property(c => c.ServiceEmployeePrecent).IsRequired();
        builder.Property(c => c.PrecentEmployeeDescription).HasMaxLength(500);
        builder.Property(c => c.EmployeeEarning).IsRequired();

        builder.HasOne(c => c.Order)
            .WithOne(c=>c.DispetcherEarning)
            .HasForeignKey<DispetcherEarning>(c => c.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

