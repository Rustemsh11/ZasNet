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
            .Include(c=>c.CreatedEmployee)
            .Include(c=>c.OrderDocuments)
            .Include(c=>c.OrderServices).ThenInclude(c=>c.OrderServiceCars).ThenInclude(c=>c.Car).ThenInclude(c => c.CarModel)
            .Include(c=>c.OrderServices).ThenInclude(c=>c.OrderServiceEmployees).ThenInclude(c=>c.Employee)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Заявки с id: {request.orderId} не найден");

        return mapper.Map<OrderDto>(order);
    }
}
