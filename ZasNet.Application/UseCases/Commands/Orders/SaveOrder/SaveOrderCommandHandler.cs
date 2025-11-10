using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Orders.SaveOrder;

public class SaveOrderCommandHandler(IRepositoryManager repositoryManager,
    IMapper mapper) : IRequestHandler<SaveOrderCommand>
{
    public async Task Handle(SaveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository.FindByCondition(c => c.Id == request.OrderDto.Id, true)
            .Include(c=>c.OrderCars)
            .Include(c=>c.OrderEmployees)
            .Include(c=>c.OrderServices)
            .SingleAsync(cancellationToken);

        var upsertOrderDto = mapper.Map<Order.UpsertOrderDto>(request.OrderDto);
        order.Update(upsertOrderDto);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
