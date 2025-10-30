namespace ZasNet.Domain.Entities;

public class OrderEmployee : BaseItem
{
    public int OrderId { get; set; }

    public int EmployeeId { get; set; }

    public Order Order { get; set; }

    public Employee Employee { get; set; }
}
