using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;

namespace ZasNet.Application.UseCases.Commands.Orders.LockOrder;

public class LockOrderCommandHandler(IRepositoryManager repositoryManager, ICurrentUserService currentUserService) 
    : IRequestHandler<LockOrderCommand>
{
    public async Task Handle(LockOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository
            .FindByCondition(o => o.Id == request.OrderId, false)
            .SingleOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Заявка с ID {request.OrderId} не найдена");
        }

        if (order.IsLocked && order.LockedByUserId != currentUserService.CurrentUserId)
        {
            var lockedByUser = await repositoryManager.EmployeeRepository
                .FindByCondition(e => e.Id == order.LockedByUserId, false)
                .SingleOrDefaultAsync(cancellationToken);
            
            throw new InvalidOperationException(
                $"Заявка уже заблокирована пользователем {lockedByUser?.Name ?? "неизвестным пользователем"}");
        }

        await repositoryManager.OrderRepository.LockItem(request.OrderId, currentUserService.CurrentUserId);
    }
}

