namespace ZasNet.Domain.Entities;

public class OrderServiceEmployee : BaseItem
{
    public int OrderServiceId { get; set; }

    public int EmployeeId { get; set; }

    public bool IsApproved { get; set; }

    public OrderService OrderService { get; set; }

    public Employee Employee { get; set; }
}
