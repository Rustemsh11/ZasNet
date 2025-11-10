using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Commands.Orders.SaveOrder;

public record SaveOrderCommand(OrderDto OrderDto):IRequest;
