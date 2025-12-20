using MediatR;

namespace ZasNet.Application.UseCases.Commands.Roles.UpdateRole;

public record UpdateRoleCommand(int Id, string Name) : IRequest;

