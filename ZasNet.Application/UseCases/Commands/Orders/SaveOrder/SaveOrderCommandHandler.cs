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
    private readonly List<OrderService> _newOrderServices = new();
    private readonly List<int> _updatedOrderServiceIds = new();

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
            await SyncOrderServices(order, upsertOrderDto.OrderServices, cancellationToken);

            // Создание EmployeeEarning для новых OrderService
            await CreateEmployeeEarningsForNewServices(order, cancellationToken);

            // Обновление EmployeeEarning для измененных OrderService
            await UpdateEmployeeEarningsForUpdatedServices(cancellationToken);

            await repositoryManager.SaveAsync(cancellationToken);
        }
        finally
        {
            await repositoryManager.OrderRepository.UnLockItem(request.OrderDto.Id);
        }

        return "";
    }

    private async Task SyncOrderServices(Order order, List<OrderService> incomingServices, CancellationToken cancellationToken)
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

        // Загружаем Services для новых OrderService
        var newServiceIds = incomingServices
            .Where(s => s.Id == 0)
            .Select(s => s.ServiceId)
            .Distinct()
            .ToList();
        
        var services = await repositoryManager.ServiceRepository
            .FindByCondition(s => newServiceIds.Contains(s.Id), false)
            .ToListAsync(cancellationToken);

        // Update existing or add new services
        foreach (var incoming in incomingServices)
        {
            var existing = existingServices.FirstOrDefault(s => s.Id == incoming.Id);
            if (existing != null)
            {
                // Check if properties that affect EmployeeEarning have changed
                bool needsEarningUpdate = existing.ServiceId != incoming.ServiceId ||
                                         existing.Price != incoming.Price ||
                                         existing.TotalVolume != incoming.TotalVolume;

                var employeesCountBefore = existing.OrderServiceEmployees.Count;

                // Update existing service properties
                existing.ServiceId = incoming.ServiceId;
                existing.Price = incoming.Price;
                existing.TotalVolume = incoming.TotalVolume;
                existing.PriceTotal = incoming.PriceTotal;

                // Sync nested collections
                SyncOrderServiceEmployees(existing, incoming.OrderServiceEmployees);
                SyncOrderServiceCars(existing, incoming.OrderServiceCars);

                // Check if employee count changed (affects earning calculation)
                var employeesCountAfter = existing.OrderServiceEmployees.Count;
                if (employeesCountAfter != employeesCountBefore)
                {
                    needsEarningUpdate = true;
                }

                // Track if we need to update EmployeeEarning
                if (needsEarningUpdate)
                {
                    _updatedOrderServiceIds.Add(existing.Id);
                }
            }
            else
            {
                // Add new service
                incoming.OrderId = order.Id;
                incoming.Order = order;
                incoming.Service = services.First(s => s.Id == incoming.ServiceId);
                order.OrderServices.Add(incoming);
                _newOrderServices.Add(incoming);
            }
        }
    }

    private Task CreateEmployeeEarningsForNewServices(Order order, CancellationToken cancellationToken)
    {
        if (_newOrderServices.Count == 0) return Task.CompletedTask;

        foreach (var orderService in _newOrderServices)
        {
            // Навигационные свойства уже установлены в SyncOrderServices
            var employeeEarning = EmployeeEarinig.CreateEmployeeEarning(orderService);
            repositoryManager.EmployeeEarningRepository.Create(employeeEarning);
        }

        return Task.CompletedTask;
    }

    private async Task UpdateEmployeeEarningsForUpdatedServices(CancellationToken cancellationToken)
    {
        if (_updatedOrderServiceIds.Count == 0) return;

        foreach (var orderServiceId in _updatedOrderServiceIds)
        {
            // Находим и удаляем старый EmployeeEarning
            var existingEarning = await repositoryManager.EmployeeEarningRepository
                .FindByCondition(ee => ee.OrderServiceId == orderServiceId, true)
                .SingleOrDefaultAsync(cancellationToken);

            if (existingEarning != null)
            {
                repositoryManager.EmployeeEarningRepository.Delete(existingEarning);
            }
        }

        // Загружаем обновленные OrderService с необходимыми данными для пересчета
        var updatedOrderServices = await repositoryManager.OrderServiceRepository
            .FindByCondition(os => _updatedOrderServiceIds.Contains(os.Id), false)
            .Include(os => os.Service)
            .Include(os => os.Order)
            .Include(os => os.OrderServiceEmployees)
            .ToListAsync(cancellationToken);

        foreach (var orderService in updatedOrderServices)
        {
            // Создаем новый EmployeeEarning с обновленными данными
            var newEmployeeEarning = EmployeeEarinig.CreateEmployeeEarning(orderService);
            repositoryManager.EmployeeEarningRepository.Create(newEmployeeEarning);
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
