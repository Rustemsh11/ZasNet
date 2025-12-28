using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence.Configurations;

namespace ZasNet.Infrastruture.Persistence;

public class ZasNetDbContext: DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<CarModel> CarModels { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<OrderServiceEmployee> OrderServiceEmployees { get; set; }
    public DbSet<OrderServiceCar> OrderServiceCars { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<OrderService> OrderServices { get; set; }
    public DbSet<Measure> Measures { get; set; }
    public DbSet<EmployeeEarinig> EmployeeEarnings { get; set; }
    public DbSet<DispetcherEarning> DispetcherEarnings { get; set; }

    public ZasNetDbContext(DbContextOptions<ZasNetDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Автоматическая конвертация всех DateTime в UTC при сохранении и обратно в Local при чтении
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => v.ToLocalTime());

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Local) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }

        var entityTypes = modelBuilder.Model.GetEntityTypes().Where(c=> typeof(BaseItem).IsAssignableFrom(c.ClrType)); // reconize what it is mean

        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType.ClrType).HasKey(nameof(BaseItem.Id));

            modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseItem.Id)).ValueGeneratedOnAdd();
        }

        modelBuilder.ApplyConfiguration(new CarConfiguration());
        modelBuilder.ApplyConfiguration(new CarModelConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderEmplyeeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderServiceConfiguration());
        modelBuilder.ApplyConfiguration(new OrderCarConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new MeasureConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeEarningConfiguration());
        modelBuilder.ApplyConfiguration(new DispetcherEarningConfiguration());
    }
}
