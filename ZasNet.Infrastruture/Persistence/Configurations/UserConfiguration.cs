using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasIndex(c=>c.Id).IsUnique();
        builder.Property(c=>c.Login).IsRequired().HasMaxLength(10);
        builder.Property(c=>c.Password).IsRequired();

        builder.HasOne(c=>c.Role).WithMany(c=>c.Users).HasForeignKey(c=>c.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
