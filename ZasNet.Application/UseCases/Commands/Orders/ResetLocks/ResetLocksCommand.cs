using MediatR;

namespace ZasNet.Application.UseCases.Commands.Orders.ResetLocks;

public record ResetLocksCommand(int OrderId) : IRequest;

