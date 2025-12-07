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
            Login = "unknown",
            Password = "changeme",
            RoleId = 3,
        });
        
        builder.HasData(new Employee()
        {
            Id = 2,
            Name = "admin",
            Login = "admin",
            Password = "$2a$11$TTmUKfiEKsy8HxE2Agg2.eVxlbn/biUtN4lloHIYuqYSovk3pl5sy",
            RoleId = 1,
        });
    }
}
