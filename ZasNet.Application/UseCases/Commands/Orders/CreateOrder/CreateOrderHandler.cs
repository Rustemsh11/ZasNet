using MediatR;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using static ZasNet.Domain.Entities.Order;

namespace ZasNet.Application.UseCases.Commands.Orders.CreateOrder;

public class CreateOrderHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateOrderCommand, Unit>
{
    public async Task<Unit> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderServices = request.OrderServicesDto.Select(c => new OrderService()
        {
            ServiceId = c.ServiceId,
            Price = c.Price,
            TotalVolume = c.TotalVolume,
            PriceTotal = c.Price * (decimal)c.TotalVolume
        }).ToList();

        var orderEmployees = request.OrderEmployeeIds.Select(c => new OrderEmployee()
        {
            EmployeeId = c
        }).ToList();
        
        var orderCars = request.OrderCarIds.Select(c => new OrderCar()
        {
            CarId = c
        }).ToList();


        var order = Order.Create(new OrderDto()
        {
            Client = request.Client,
            Date = request.Date,
            AddressCity = request.AddressCity,
            AddressStreet = request.AddressStreet,
            AddressNumber = request.AddressNumber,
            OrderPriceAmount = request.OrderPriceAmount,
            PaymentType = request.PaymentType,
            Description = request.Description,
            OrderCars = orderCars,
            OrderEmployees = orderEmployees,
            OrderServices = orderServices,
            ClientType = request.ClientType,
            CreatedEmployeeId = request.CreatedUserId,
        });
        
        repositoryManager.OrderRepository.Create(order);

        await repositoryManager.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
