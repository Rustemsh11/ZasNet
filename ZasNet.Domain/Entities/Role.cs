namespace ZasNet.Domain.Entities;

public class Role: LockedItemBase
{
    public string Name { get; set; }

    public ICollection<Employee> Employees { get; set; }

    public static Role Create(string name)
    {
        return new Role
        {
            Name = name
        };
    }

    public void UpdateRole(string name)
    {
        this.Name = name;
    }
}
