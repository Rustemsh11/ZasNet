namespace ZasNet.Application.CommonDtos;

public class OrderServiceEmployeeDto
{
    public int OrderServiceId { get; set; }
    public EmployeeDto Employee { get; set; }

    public bool IsApproved { get; set; }
}
