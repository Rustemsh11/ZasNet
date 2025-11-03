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
        var clientType = new EnumToNumberConverter<ClientType, short>();
        builder.ToTable("Orders");
        builder.Property(c => c.Client).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Date).IsRequired();
        builder.Property(c => c.AddressCity).IsRequired().HasMaxLength(100);
        builder.Property(c => c.AddressStreet).IsRequired().HasMaxLength(150);
        builder.Property(c => c.AddressNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.OrderPriceAmount).IsRequired();
        builder.Property(c => c.PaymentType).HasConversion(paymentTypeConverter).IsRequired();
        builder.Property(c => c.Status).HasConversion(statusConverter).IsRequired();
        builder.Property(c => c.ClientType).HasConversion(clientType).IsRequired();
        builder.Property(c => c.CreatedDate).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}
