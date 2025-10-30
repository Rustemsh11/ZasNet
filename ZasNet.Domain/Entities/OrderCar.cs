namespace ZasNet.Domain.Entities;

public class OrderCar : BaseItem
{
    public int OrderId { get; set; }

    public int CarId { get; set; }

    public Order Order { get; set; }

    public Car Car { get; set; }
}
