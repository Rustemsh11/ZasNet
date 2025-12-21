using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Orders.DeleteOrder;

public class DeleteOrderHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            throw new ArgumentException($"Заявки с id {request.Id} не найдено");
        }
        
        if(order.Status == Domain.Enums.OrderStatus.AwaitingPayment || order.Status == Domain.Enums.OrderStatus.Closed)
        {
            throw new InvalidOperationException("Заявку в статусе [Ожидает оплату] или [Закрыт] нельзя удалить");
        }

        repositoryManager.OrderRepository.Delete(order);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
