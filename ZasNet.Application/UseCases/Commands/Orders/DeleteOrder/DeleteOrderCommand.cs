using MediatR;

namespace ZasNet.Application.UseCases.Commands.Orders.DeleteOrder;

public record DeleteOrderCommand(int Id) : IRequest;
