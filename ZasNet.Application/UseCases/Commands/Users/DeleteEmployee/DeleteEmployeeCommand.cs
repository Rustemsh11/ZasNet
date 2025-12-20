using MediatR;

namespace ZasNet.Application.UseCases.Commands.Users.DeleteEmployee;

public record DeleteEmployeeCommand(int Id): IRequest;
