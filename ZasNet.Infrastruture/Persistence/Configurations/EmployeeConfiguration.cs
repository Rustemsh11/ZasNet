using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.Property(c=>c.Name).IsRequired();

        builder.Property(c => c.Login).IsRequired().HasMaxLength(10);
        builder.Property(c => c.Password).IsRequired();

        builder.HasOne(c => c.Role).WithMany(c => c.Employees).HasForeignKey(c => c.RoleId);

        builder.HasData(new Employee()
        {
            Id = 1,
            Name = "Не известно",
            RoleId = 3,
        });
    }
}
