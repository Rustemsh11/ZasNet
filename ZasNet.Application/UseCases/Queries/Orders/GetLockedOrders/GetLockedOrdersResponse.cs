namespace ZasNet.Application.UseCases.Queries.Orders.GetLockedOrders;

public class GetLockedOrdersResponse
{
	public int Id { get; set; }

	public string OrderClientName { get; set; }

	public DateTime OrderDate { get; set; }

	public string? LockedByUserName { get; set; }

	public DateTime? LockedAt { get; set; }
}

