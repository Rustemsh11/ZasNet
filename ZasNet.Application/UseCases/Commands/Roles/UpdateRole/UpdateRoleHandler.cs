using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Roles.UpdateRole;

public class UpdateRoleHandler(IRepositoryManager repositoryManager) : IRequestHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleRepo = repositoryManager.RoleRepository;

        var role = await roleRepo.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException($"Роли с id: {request.Id} не существует");
        }

        role.UpdateRole(request.Name);

        roleRepo.Update(role);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

