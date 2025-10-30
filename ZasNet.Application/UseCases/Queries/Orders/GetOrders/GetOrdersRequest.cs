using MediatR;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrders;

public record GetOrdersRequest(): IRequest<List<GetOrdersResponse>>;
