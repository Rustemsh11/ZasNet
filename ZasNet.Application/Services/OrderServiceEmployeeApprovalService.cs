using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using ZasNet.Application.Repository;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Interfaces;

namespace ZasNet.Application.Services;

public class OrderServiceEmployeeApprovalService(IRepositoryManager repositoryManager) : IOrderServiceEmployeeApprovalService
{
    public async Task<ApprovalResult> ApproveAssignedAsync(int orderServiceEmployeeId, CancellationToken cancellationToken)
    {
        // Load service employee with related order + service for messages
        var serviceEmployee = await repositoryManager.OrderEmployeeRepository
            .FindByCondition(c => c.Id == orderServiceEmployeeId, true)
            .Include(c => c.OrderService)
            .ThenInclude(c => c.Service)
            .SingleOrDefaultAsync(cancellationToken);

        if (serviceEmployee == null)
        {
            return new ApprovalResult
            {
                Success = false,
                Message = "Не удалось подтвердить услугу. Сотрудник услуги не найден. Попробуйте обновить список"
            };
        }

        var orderId = serviceEmployee.OrderService.OrderId;
        
        var order = await repositoryManager.OrderRepository
            .FindByCondition(c => c.Id == orderId, true)
            .SingleAsync(cancellationToken);
        if (order.Status != OrderStatus.Created) 
        {
            return new ApprovalResult
            {
                Success = false,
                Message = "Данная заявка не в статусе создан"
            };
        }

        var lockedBy = order.IsLocked;

        if (lockedBy)
        {
            var lockedEmployee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == order.LockedByUserId, false).Select(c => c.Name).SingleOrDefaultAsync(cancellationToken);
            return new ApprovalResult
            {
                Success = false,
                Message = $"Заявку редактирует {lockedEmployee}. Через некоторое время обновите список заявок и повторите операцию"
            };
        }

        await repositoryManager.OrderRepository.LockItem(orderId, serviceEmployee.EmployeeId);
        try
        {
            serviceEmployee.IsApproved = true;
            await repositoryManager.SaveAsync(cancellationToken);

            await UpdateOrderStatusAfterEmployeeApprovalAsync(orderId, serviceEmployee.Id, cancellationToken);

            return new ApprovalResult
            {
                Success = true,
                Message = $"Услуга:[{serviceEmployee.OrderService.Service.Name}] успешно подтверждена"
            };
        }
        finally
        {
            await repositoryManager.OrderRepository.UnLockItem(orderId);
        }
    }

    public async Task UpdateOrderStatusAfterEmployeeApprovalAsync(int orderId, int approvedOrderServiceEmployeeId, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository
            .FindByCondition(c => c.Id == orderId, true)
            .SingleAsync(cancellationToken);

        var anyNotApprovedExceptCurrent = await repositoryManager.OrderEmployeeRepository
            .FindByCondition(c => c.OrderService.OrderId == orderId && c.Id != approvedOrderServiceEmployeeId && !c.IsApproved, false)
            .AnyAsync(cancellationToken);

        if (!anyNotApprovedExceptCurrent)
        {
            order.UpdateStatus(OrderStatus.ApprovedWithEmployers);
        }
    }
}


