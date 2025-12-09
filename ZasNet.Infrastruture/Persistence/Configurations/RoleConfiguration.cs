using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.Property(c=>c.Name).IsRequired();

        builder.HasData(new Role()
        {
            Id = 1,
            Name = "admin",
        },
        new Role()
        {
            Id = 2,
            Name = "Диспетчер"
        },
        new Role()
        {
            Id = 3,
            Name = "Водитель"
        },
        new Role()
        {
            Id = 4,
            Name = "Бухгалтер"
        });
    }
}
