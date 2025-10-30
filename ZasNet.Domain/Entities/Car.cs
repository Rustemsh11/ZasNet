using ZasNet.Domain.Enums;

namespace ZasNet.Domain.Entities;

public class Car : LockedItemBase
{
    public int? CarModelId { get; set; }

    public string Number { get; set; }

    public CarStatus Status { get; set; }

    public CarModel CarModel { get; set; }

    public ICollection<OrderCar> OrderCars { get; set; }

}
