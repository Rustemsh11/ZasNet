using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        var paymentTypeConverter = new EnumToNumberConverter<PaymentType, short>();
        var statusConverter = new EnumToNumberConverter<OrderStatus, short>();
        builder.ToTable("Orders");
        builder.Property(c => c.Client).IsRequired().HasMaxLength(200);
        builder.Property(c => c.DateStart).IsRequired();
        builder.Property(c => c.DateEnd).IsRequired();
        builder.Property(c => c.AddressCity).IsRequired().HasMaxLength(100);
        builder.Property(c => c.AddressStreet).IsRequired().HasMaxLength(150);
        builder.Property(c => c.AddressNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.OrderPriceAmount).IsRequired();
        builder.Property(c => c.PaymentType).HasConversion(paymentTypeConverter).IsRequired();
        builder.Property(c => c.Status).HasConversion(statusConverter).IsRequired();
        builder.Property(c => c.CreatedDate).IsRequired().HasDefaultValueSql("GETDATE()");
        builder.Property(c => c.CreatedEmployeeId).HasDefaultValue(1);

        builder.HasOne(c=>c.CreatedEmployee).WithMany(c=>c.CreatedByEmployeeOrder).HasForeignKey(c => c.CreatedEmployeeId);

		// Finished by employee (optional)
		builder.HasOne(c => c.FinishedEmployee)
			.WithMany()
			.HasForeignKey(c => c.FinishedEmployeeId)
			.OnDelete(DeleteBehavior.Restrict);
    }
}
