using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrders;

public class GetOrdersResponse
{
    public int Id { get; set; }

    public string Client {  get; set; }

    public DateTime Date { get; set; }

    public OrderStatus Status { get; set; }

}
