using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Orders.ResetLocks;

public class ResetLocksCommandHandler(IRepositoryManager repositoryManager) : IRequestHandler<ResetLocksCommand>
{
	public async Task Handle(ResetLocksCommand request, CancellationToken cancellationToken)
	{
		var order = await repositoryManager.OrderRepository
			.FindByCondition(o => o.Id == request.OrderId, false)
			.SingleOrDefaultAsync(cancellationToken);

		if (order == null)
		{
			return;
		}

		if (order.LockedByUserId == null)
		{
			return;
		}

		await repositoryManager.OrderRepository.UnLockItem(request.OrderId);

		return;
	}
}

  