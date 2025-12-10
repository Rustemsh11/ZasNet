using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Orders.SaveOrder;

public class SaveOrderCommandHandler(IRepositoryManager repositoryManager,
    ICurrentUserService currentUserService,
    IMapper mapper) : IRequestHandler<SaveOrderCommand, string>
{
    public async Task<string> Handle(SaveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository.FindByCondition(c => c.Id == request.OrderDto.Id, true)
            .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceCars)
            .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceEmployees)
            .SingleAsync(cancellationToken);

        if (order.IsLocked)
        {
            var user = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == order.LockedByUserId, false).SingleOrDefaultAsync();
            return $"Заявку редактирует {user.Name}. Попробуйте попытку через пару минут";
        }

        await repositoryManager.OrderRepository.LockItem(request.OrderDto.Id, currentUserService.CurrentUserId);
        try
        {
            var upsertOrderDto = mapper.Map<Order.UpsertOrderDto>(request.OrderDto);

            // Update order properties
            order.Update(upsertOrderDto);

            // Sync OrderServices
            SyncOrderServices(order, upsertOrderDto.OrderServices);

            await repositoryManager.SaveAsync(cancellationToken);
        }
        finally
        {
            await repositoryManager.OrderRepository.UnLockItem(request.OrderDto.Id);
        }

        return "";
    }

    private void SyncOrderServices(Order order, List<OrderService> incomingServices)
    {
        var existingServices = order.OrderServices.ToList();

        // Remove services that are no longer present
        foreach (var existing in existingServices)
        {
            if (!incomingServices.Any(s => s.Id == existing.Id))
            {
                order.OrderServices.Remove(existing);
            }
        }

        // Update existing or add new services
        foreach (var incoming in incomingServices)
        {
            var existing = existingServices.FirstOrDefault(s => s.Id == incoming.Id);
            if (existing != null)
            {
                // Update existing service properties
                existing.ServiceId = incoming.ServiceId;
                existing.Price = incoming.Price;
                existing.TotalVolume = incoming.TotalVolume;
                existing.PriceTotal = incoming.PriceTotal;

                // Sync nested collections
                SyncOrderServiceEmployees(existing, incoming.OrderServiceEmployees);
                SyncOrderServiceCars(existing, incoming.OrderServiceCars);
            }
            else
            {
                // Add new service
                incoming.OrderId = order.Id;
                order.OrderServices.Add(incoming);
            }
        }
    }

    private void SyncOrderServiceEmployees(OrderService orderService, ICollection<OrderServiceEmployee> incomingEmployees)
    {
        var existingEmployees = orderService.OrderServiceEmployees.ToList();

        // Remove employees that are no longer present (compare by EmployeeId)
        foreach (var existing in existingEmployees)
        {
            if (!incomingEmployees.Any(e => e.EmployeeId == existing.EmployeeId))
            {
                orderService.OrderServiceEmployees.Remove(existing);
            }
        }

        // Update existing or add new employees
        foreach (var incoming in incomingEmployees)
        {
            var existing = existingEmployees.FirstOrDefault(e => e.EmployeeId == incoming.EmployeeId);
            if (existing != null)
            {
                // Update properties of existing employee 
                existing.IsApproved = incoming.IsApproved;
            }
            else
            {
                // Add new employee
                incoming.OrderServiceId = orderService.Id;
                orderService.OrderServiceEmployees.Add(incoming);
            }
        }
    }

    private void SyncOrderServiceCars(OrderService orderService, ICollection<OrderServiceCar> incomingCars)
    {
        var existingCars = orderService.OrderServiceCars.ToList();

        // Remove cars that are no longer present (compare by CarId)
        foreach (var existing in existingCars)
        {
            if (!incomingCars.Any(c => c.CarId == existing.CarId))
            {
                orderService.OrderServiceCars.Remove(existing);
            }
        }

        // Update existing or add new cars
        foreach (var incoming in incomingCars)
        {
            var existing = existingCars.FirstOrDefault(c => c.CarId == incoming.CarId);
            if (existing != null)
            {
                // Update properties of existing car
                existing.IsApproved = incoming.IsApproved;
            }
            else
            {
                // Add new car
                incoming.OrderServiceId = orderService.Id;
                orderService.OrderServiceCars.Add(incoming);
            }
        }
    }
}
