namespace ZasNet.Domain.Entities;

public abstract class BaseItem
{
    public int Id { get; set; }

    public bool? IsDeleted { get; set; }
}
