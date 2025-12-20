using MediatR;

namespace ZasNet.Application.UseCases.Commands.Roles.CreateRole;

public record CreateRoleRequest(string Name) : IRequest;

