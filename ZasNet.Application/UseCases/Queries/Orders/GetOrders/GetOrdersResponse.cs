using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrders;

public class GetOrdersResponse
{
    public int Id { get; set; }

    public string Client {  get; set; }

    public DateTime Date { get; set; }

    public string Address { get; set; }

    public string Employee {  get; set; }

    public OrderStatus Status { get; set; }

    public List<string> ServiceNames {  get; set; }

}
