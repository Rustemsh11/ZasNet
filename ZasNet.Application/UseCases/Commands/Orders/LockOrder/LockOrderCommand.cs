using MediatR;

namespace ZasNet.Application.UseCases.Commands.Orders.LockOrder;

public record LockOrderCommand(int OrderId) : IRequest;

