using MediatR;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;
using static ZasNet.Domain.Entities.Order;

namespace ZasNet.Application.UseCases.Commands.Orders.CreateOrder;

public class CreateOrderHandler(
    IRepositoryManager repositoryManager,
    IOrderNotificationService orderNotificationService) : IRequestHandler<CreateOrderCommand>
{
    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderServices = request.OrderDto.OrderServicesDtos.Select(c => new OrderService()
        {
            ServiceId = c.ServiceId,
            Price = c.Price,
            TotalVolume = c.TotalVolume,
            PriceTotal = c.Price * (decimal)c.TotalVolume,
            OrderServiceCars = c.OrderServiceCarDtos
                .Select(x => new OrderServiceCar
                {
                    CarId = x.Car.Id
                })
                .ToList(),
            OrderServiceEmployees = c.OrderServiceEmployeeDtos
                .Select(x => new OrderServiceEmployee
                {
                    EmployeeId = x.Employee.Id
                })
                .ToList(),
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
            OrderServices = orderServices,
            ClientType = request.OrderDto.ClientType,
            CreatedEmployeeId = request.OrderDto.CreatedUser.Id,
        });
        
        repositoryManager.OrderRepository.Create(order);
        await repositoryManager.SaveAsync(cancellationToken);
        await orderNotificationService.NotifyOrderCreatedAsync(order, cancellationToken);
    }
}
