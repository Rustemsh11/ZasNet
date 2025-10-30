namespace ZasNet.Application.UseCases.Queries.Orders.GetOrders;

public class GetOrdersResponse
{
    public string Client {  get; set; }

    public DateTime Date { get; set; }

    public string Address { get; set; }

    public string Employee {  get; set; }

    public List<string> ServiceNames {  get; set; }

}
