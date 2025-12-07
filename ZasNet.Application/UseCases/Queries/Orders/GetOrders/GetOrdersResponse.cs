using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrders;

public class GetOrdersResponse
{
    public int Id { get; set; }

    public string Client {  get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }
    
    public string Address { get; set; }

    public OrderStatus Status { get; set; }
    
    public List<string?> CarNames { get; set; }
    
    public List<int?> CarIds { get; set; }

}
