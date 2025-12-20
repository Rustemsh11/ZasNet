using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Roles.GetRoles;

public class GetRolesHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetRolesRequest, List<RoleDto>>
{
    public async Task<List<RoleDto>> Handle(GetRolesRequest request, CancellationToken cancellationToken)
    {
        return await repositoryManager.RoleRepository.FindAll(false)
            .Select(c => new RoleDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);
    }
}

