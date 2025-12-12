using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Entities;
using static ZasNet.Domain.Entities.Order;

namespace ZasNet.Application.UseCases.Commands.Orders.CreateOrder;

public class CreateOrderHandler(
    IRepositoryManager repositoryManager,
    IOrderNotificationService orderNotificationService) : IRequestHandler<CreateOrderCommand>
{
    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderServices = request.OrderDto.OrderServicesDtos.Select(c =>
        {
            var orderService = new OrderService()
            {
                ServiceId = c.ServiceId,
                Price = c.Price,
                TotalVolume = c.TotalVolume,
                PriceTotal = c.Price * (decimal)c.TotalVolume,
                OrderServiceCars = c.OrderServiceCarDtos
                    .Select(x => new OrderServiceCar
                    {
                        CarId = x.Car.Id,
                        IsApproved = x.IsApproved,
                    })
                    .ToList(),
                OrderServiceEmployees = c.OrderServiceEmployeeDtos
                    .Select(x => new OrderServiceEmployee
                    {
                        EmployeeId = x.Employee.Id,
                        IsApproved = x.IsApproved,
                    })
                    .ToList(),
            };
            
            return orderService;
        }).ToList();



        // Загружаем Service для каждого OrderService до создания заказа
        var serviceIds = request.OrderDto.OrderServicesDtos.Select(dto => dto.ServiceId).Distinct().ToList();
        var services = await repositoryManager.ServiceRepository
            .FindByCondition(s => serviceIds.Contains(s.Id), false)
            .ToListAsync(cancellationToken);

        var order = Order.Create(new UpsertOrderDto()
        {
            Client = request.OrderDto.Client,
            DateStart = request.OrderDto.DateStart,
            DateEnd = request.OrderDto.DateEnd,
            AddressCity = request.OrderDto.AddressCity,
            AddressStreet = request.OrderDto.AddressStreet,
            AddressNumber = request.OrderDto.AddressNumber,
            OrderPriceAmount = request.OrderDto.OrderPriceAmount,
            PaymentType = request.OrderDto.PaymentType,
            Description = request.OrderDto.Description,
            OrderServices = orderServices,
            Status = request.OrderDto.Status,
            CreatedEmployeeId = request.OrderDto.CreatedUser.Id,
        });
        
        repositoryManager.OrderRepository.Create(order);

        // Создание EmployeeEarning для каждого OrderService в той же транзакции
        foreach (var orderService in orderServices)
        {
            // Устанавливаем навигационные свойства вручную
            orderService.Order = order;
            orderService.Service = services.First(s => s.Id == orderService.ServiceId);

            var employeeEarning = EmployeeEarinig.CreateEmployeeEarning(orderService);
            repositoryManager.EmployeeEarningRepository.Create(employeeEarning);
        }
        
        await repositoryManager.SaveAsync(cancellationToken);

		var assignedEmployeeIds = orderServices
			.SelectMany(s => s.OrderServiceEmployees)
			.Select(ose => ose.EmployeeId)
			.Distinct()
			.ToList();

		if (assignedEmployeeIds.Count > 0)
		{
			// Load required navigation properties before notification
			order = await repositoryManager.OrderRepository
				.FindByCondition(o => o.Id == order.Id, false)
				.Include(o => o.OrderServices).ThenInclude(os => os.Service)
				.Include(o => o.OrderServices).ThenInclude(os => os.OrderServiceEmployees).ThenInclude(ose => ose.Employee)
				.Include(o => o.OrderServices).ThenInclude(os => os.OrderServiceCars).ThenInclude(osc => osc.Car).ThenInclude(c => c.CarModel)
				.SingleAsync(cancellationToken);

			var chatIds = repositoryManager.EmployeeRepository
				.FindByCondition(e => assignedEmployeeIds.Contains(e.Id) && e.ChatId != null, false)
				.Select(e => e.ChatId!.Value)
				.Distinct()
				.ToList();

			foreach (var chatId in chatIds)
			{
				await orderNotificationService.NotifyOrderCreatedAsync(order, chatId, cancellationToken);
			}
		}
    }
}
