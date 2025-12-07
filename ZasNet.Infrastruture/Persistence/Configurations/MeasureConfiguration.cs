using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Persistence.Configurations;

public class MeasureConfiguration : IEntityTypeConfiguration<Measure>
{
    public void Configure(EntityTypeBuilder<Measure> builder)
    {
        builder.HasData(new Measure()
        {
            Id = 1,
            Name = "метр",
        });
        
        builder.HasData(new Measure()
        {
            Id = 2,
            Name = "час",
        });
    }
}
