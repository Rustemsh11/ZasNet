using Microsoft.EntityFrameworkCore;
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
    public DbSet<User> User { get; set; }

    public ZasNetDbContext(DbContextOptions<ZasNetDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
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
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }
}
