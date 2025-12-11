using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;

namespace ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatus;

public class ChangeOrderStatusHandler(IRepositoryManager repositoryManager, ICurrentUserService currentUserService) : IRequestHandler<ChangeOrderStatusCommand>
{
    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository.FindByCondition(c=>c.Id == request.OrderId, true).SingleAsync(cancellationToken);
        
        try
        {
            order.UpdateStatus(request.OrderStatus);
            await repositoryManager.OrderRepository.LockItem(request.OrderId, currentUserService.CurrentUserId);
            await repositoryManager.SaveAsync(cancellationToken);
        }
        finally
        {
            await repositoryManager.OrderRepository.UnLockItem(request.OrderId);
        }

        return;
    }
}
