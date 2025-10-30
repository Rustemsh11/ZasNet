namespace ZasNet.Domain.Entities;

public class Role: LockedItemBase
{
    public string Name { get; set; }

    public ICollection<User> Users { get; set; }
}
