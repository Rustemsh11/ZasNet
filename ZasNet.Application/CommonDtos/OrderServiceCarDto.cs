namespace ZasNet.Application.CommonDtos;

public class OrderServiceCarDto
{
    public int OrderServiceId { get; set; }
    public CarDto Car { get; set; }

    public bool IsApproved { get; set; }
}
