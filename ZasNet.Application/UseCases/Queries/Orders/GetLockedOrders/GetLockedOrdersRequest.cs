using MediatR;

namespace ZasNet.Application.UseCases.Queries.Orders.GetLockedOrders;

public record GetLockedOrdersRequest() : IRequest<List<GetLockedOrdersResponse>>;

