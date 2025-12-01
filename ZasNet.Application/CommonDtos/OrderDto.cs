using ZasNet.Domain.Enums;

namespace ZasNet.Application.CommonDtos;

public class OrderDto
{
    public int Id { get; set; }

    public string Client { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string AddressCity { get; set; }

    public string AddressStreet { get; set; }

    public string AddressNumber { get; set; }

    public decimal OrderPriceAmount { get; set; }

    public PaymentType PaymentType { get; set; }

    public OrderStatus Status { get; set; }

    public string? Description { get; set; }

    public EmployeeDto CreatedUser { get; set; }

    public List<OrderServiceDto> OrderServicesDtos { get; set; }

    public List<DocumentDto> Documents { get; set; }
}
