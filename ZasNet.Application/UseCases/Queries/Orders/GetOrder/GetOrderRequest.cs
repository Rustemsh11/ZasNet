using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrder;

public record GetOrderRequest(int orderId): IRequest<OrderDto>;

