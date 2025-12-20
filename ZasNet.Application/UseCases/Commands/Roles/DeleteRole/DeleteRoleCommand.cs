using MediatR;

namespace ZasNet.Application.UseCases.Commands.Roles.DeleteRole;

public record DeleteRoleCommand(int Id): IRequest;

