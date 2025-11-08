using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Commands.Orders.CreateOrder;

public record CreateOrderCommand(OrderDto orderDto): IRequest;
