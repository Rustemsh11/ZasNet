using ZasNet.Domain.Enums;

namespace ZasNet.Domain.Entities;

public class Car : LockedItemBase
{
    public int? CarModelId { get; set; }

    public string Number { get; set; }

    public CarStatus Status { get; set; }

    public CarModel CarModel { get; set; }

    public ICollection<OrderServiceCar> OrderServiceCars { get; set; }

    public static Car Create(string number, CarStatus status, int? carModelId)
    {
        return new Car
        {
            Number = number,
            Status = status,
            CarModelId = carModelId
        };
    }

    public void UpdateCar(string number, CarStatus status, int? carModelId)
    {
        this.Number = number;
        this.Status = status;
        this.CarModelId = carModelId;
    }
}
