namespace ZasNet.Domain.Entities;

public class Measure: BaseItem
{
    public string Name { get; set; }

    public ICollection<Service> Services { get; set; }
}
