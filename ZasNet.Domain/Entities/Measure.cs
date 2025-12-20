namespace ZasNet.Domain.Entities;

public class Measure: BaseItem
{
    public string Name { get; set; }

    public ICollection<Service> Services { get; set; }

    public static Measure Create(string name)
    {
        return new Measure
        {
            Name = name
        };
    }

    public void UpdateMeasure(string name)
    {
        this.Name = name;
    }
}
