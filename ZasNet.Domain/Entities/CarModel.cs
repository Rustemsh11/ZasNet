namespace ZasNet.Domain.Entities;

public class CarModel : LockedItemBase
{
    public string Name { get; set; }

    public ICollection<Car> Cars { get; set;}
}
