using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Roles.DeleteRole;

public class DeleteRoleHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await repositoryManager.RoleRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Роли с id [{request.Id}] не найдено");

        role.IsDeleted = true;

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

