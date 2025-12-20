using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Roles.CreateRole;

public class CreateRoleHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateRoleRequest>
{
    public async Task Handle(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var roleRepo = repositoryManager.RoleRepository;

        var matchs = await roleRepo.FindByCondition(c => c.Name.ToLower() == request.Name.ToLower(), false).ToListAsync(cancellationToken);
        if(matchs.Count > 0)
        {
            throw new InvalidOperationException("Роль с таким названием уже существует");
        }

        var role = Role.Create(request.Name);

        roleRepo.Create(role);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

