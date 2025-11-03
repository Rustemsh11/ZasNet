using MediatR;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Commands.Orders.CreateOrder;

public class CreateOrderCommand: IRequest<Unit>
{
    public string Client {  get; set; }

    public DateTime Date { get; set; }

    public string AddressCity { get; set; }

    public string AddressStreet { get; set; }

    public string AddressNumber { get; set; }

    public decimal OrderPriceAmount { get; set; }

    public PaymentType PaymentType { get; set; }
    
    public ClientType ClientType { get; set; }

    public string? Description { get; set; }

    public List<int> OrderEmployeeIds { get; set; }

    public List<int> OrderCarIds { get; set; }

    public List<OrderServicesDto> OrderServicesDto { get; set; }

}
