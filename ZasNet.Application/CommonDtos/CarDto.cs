namespace ZasNet.Application.CommonDtos;

public class CarDto
{
    public int Id { get; set; }

    public string Number { get; set; }

    public int Status { get; set; }

    public CarModelDto CarModel { get; set; }
}
