using MediatR;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatus;

public record ChangeOrderStatusCommand(int OrderId, OrderStatus OrderStatus): IRequest;
