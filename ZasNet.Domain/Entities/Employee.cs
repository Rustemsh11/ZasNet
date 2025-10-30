namespace ZasNet.Domain.Entities;

public class Employee : LockedItemBase
{
    public string Name { get; set; }

    public string? Phone { get; set; }

    public ICollection<OrderEmployee> OrderEmployees { get; set; }

}
