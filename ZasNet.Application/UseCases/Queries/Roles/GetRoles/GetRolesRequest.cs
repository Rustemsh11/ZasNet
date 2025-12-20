using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Roles.GetRoles;

public record GetRolesRequest() : IRequest<List<RoleDto>>;

