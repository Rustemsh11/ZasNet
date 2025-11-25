namespace ZasNet.Domain.Entities;

public class Role: LockedItemBase
{
    public string Name { get; set; }

    public ICollection<Employee> Employees { get; set; }
}
