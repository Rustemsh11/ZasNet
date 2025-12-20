namespace ZasNet.Domain.Entities;

public class OrderService : LockedItemBase
{
    public int OrderId { get; set; }
    
    public int ServiceId { get; set; }

    public decimal Price { get; set; }

    public double TotalVolume { get; set; }

    public decimal PriceTotal { get; set;}

    public bool? IsAlmazService { get; set; }

    public Order Order { get; set; }

    public Service Service { get; set; }

    public ICollection<OrderServiceCar> OrderServiceCars { get; set; }

    public ICollection<OrderServiceEmployee> OrderServiceEmployees { get; set; }

    public EmployeeEarinig EmployeeEarinig { get; set; }
}
