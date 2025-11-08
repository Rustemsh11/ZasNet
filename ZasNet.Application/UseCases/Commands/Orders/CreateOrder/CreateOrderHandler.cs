using MediatR;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using static ZasNet.Domain.Entities.Order;

namespace ZasNet.Application.UseCases.Commands.Orders.CreateOrder;

public class CreateOrderHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateOrderCommand>
{
    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderServices = request.orderDto.OrderServicesDtos.Select(c => new OrderService()
        {
            ServiceId = c.ServiceId,
            Price = c.Price,
            TotalVolume = c.TotalVolume,
            PriceTotal = c.Price * (decimal)c.TotalVolume
        }).ToList();

        var orderEmployees = request.orderDto.OrderEmployeeDtos.Select(c => new OrderEmployee()
        {
            EmployeeId = c.Id
        }).ToList();
        
        var orderCars = request.orderDto.OrderCarDtos.Select(c => new OrderCar()
        {
            CarId = c.Id
        }).ToList();


        var order = Order.Create(new CreateOrderDto()
        {
            Client = request.orderDto.Client,
            Date = request.orderDto.Date,
            AddressCity = request.orderDto.AddressCity,
            AddressStreet = request.orderDto.AddressStreet,
            AddressNumber = request.orderDto.AddressNumber,
            OrderPriceAmount = request.orderDto.OrderPriceAmount,
            PaymentType = request.orderDto.PaymentType,
            Description = request.orderDto.Description,
            OrderCars = orderCars,
            OrderEmployees = orderEmployees,
            OrderServices = orderServices,
            ClientType = request.orderDto.ClientType,
            CreatedEmployeeId = request.orderDto.CreatedUserId,
        });
        
        repositoryManager.OrderRepository.Create(order);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
