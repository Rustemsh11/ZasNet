namespace ZasNet.Domain.Entities;

public class CarModel : LockedItemBase
{
    public string Name { get; set; }

    public ICollection<Car> Cars { get; set;}

    public static CarModel Create(string name)
    {
        return new CarModel
        {
            Name = name
        };
    }

    public void UpdateCarModel(string name)
    {
        this.Name = name;
    }
}
