using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrder;

public class GetOrderHandler(IRepositoryManager repositoryManager,
    IMapper mapper) : IRequestHandler<GetOrderRequest, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository.FindByCondition(c => c.Id == request.orderId, false)
            .Include(c=>c.OrderCars).ThenInclude(c=>c.Car).ThenInclude(c=>c.CarModel)
            .Include(c=>c.OrderEmployees).ThenInclude(c=>c.Employee)
            .Include(c=>c.OrderServices)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Заявки с id: {request.orderId} не найден");

        return mapper.Map<OrderDto>(order);
    }
}
