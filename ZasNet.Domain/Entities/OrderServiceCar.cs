namespace ZasNet.Domain.Entities;

public class OrderServiceCar : BaseItem
{
    public int OrderServiceId { get; set; }

    public int CarId { get; set; }

    public bool IsApproved { get; set; }

    public OrderService OrderService { get; set; }

    public Car Car { get; set; }
}
