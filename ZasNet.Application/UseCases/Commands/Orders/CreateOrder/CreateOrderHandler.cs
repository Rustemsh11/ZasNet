using MediatR;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using static ZasNet.Domain.Entities.Order;

namespace ZasNet.Application.UseCases.Commands.Orders.CreateOrder;

public class CreateOrderHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateOrderCommand>
{
    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderServices = request.OrderDto.OrderServicesDtos.Select(c => new OrderService()
        {
            ServiceId = c.ServiceId,
            Price = c.Price,
            TotalVolume = c.TotalVolume,
            PriceTotal = c.Price * (decimal)c.TotalVolume
        }).ToList();

        var orderEmployees = request.OrderDto.OrderEmployeeDtos.Select(c => new OrderEmployee()
        {
            EmployeeId = c.Id
        }).ToList();
        
        var orderCars = request.OrderDto.OrderCarDtos.Select(c => new OrderCar()
        {
            CarId = c.Id
        }).ToList();


        var order = Order.Create(new UpsertOrderDto()
        {
            Client = request.OrderDto.Client,
            Date = request.OrderDto.Date,
            AddressCity = request.OrderDto.AddressCity,
            AddressStreet = request.OrderDto.AddressStreet,
            AddressNumber = request.OrderDto.AddressNumber,
            OrderPriceAmount = request.OrderDto.OrderPriceAmount,
            PaymentType = request.OrderDto.PaymentType,
            Description = request.OrderDto.Description,
            OrderCars = orderCars,
            OrderEmployees = orderEmployees,
            OrderServices = orderServices,
            ClientType = request.OrderDto.ClientType,
            CreatedEmployeeId = request.OrderDto.CreatedUser.Id,
        });
        
        repositoryManager.OrderRepository.Create(order);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
