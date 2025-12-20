using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;
using static ZasNet.Domain.Entities.EmployeeEarinig;

namespace ZasNet.Application.UseCases.Commands.Orders.SaveOrder;

public class SaveOrderCommandHandler(IRepositoryManager repositoryManager,
    ICurrentUserService currentUserService,
    IMapper mapper) : IRequestHandler<SaveOrderCommand, string>
{
    private readonly List<(OrderService orderService, OrderServiceDto dto)> _newOrderServices = new();
    private readonly List<(int orderServiceId, OrderServiceDto dto)> _updatedOrderServices = new();

    public async Task<string> Handle(SaveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository.FindByCondition(c => c.Id == request.OrderDto.Id, true)
            .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceCars)
            .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceEmployees)
            .Include(c=>c.DispetcherEarning)
            .SingleAsync(cancellationToken);

        if (order.IsLocked)
        {
            var user = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == order.LockedByUserId, false).SingleOrDefaultAsync();
            return $"Заявку редактирует {user?.Name ?? "другой пользователь"}. Попробуйте попытку через пару минут";
        }

        await repositoryManager.OrderRepository.LockItem(request.OrderDto.Id, currentUserService.CurrentUserId);
        try
        {
            var upsertOrderDto = mapper.Map<Order.UpsertOrderDto>(request.OrderDto);

            // Update order properties
            order.Update(upsertOrderDto);
            var dispetcherProcent = (await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == request.OrderDto.CreatedUser.Id, false).SingleOrDefaultAsync(cancellationToken))?.DispetcherProcent;
            order.DispetcherEarning.UpdateDispetcherEarning(dispetcherProcent.Value, order.OrderPriceAmount);
            
            // Sync OrderServices
            await SyncOrderServices(order, upsertOrderDto.OrderServices, request.OrderDto.OrderServicesDtos, cancellationToken);

            // Создание EmployeeEarning для новых OrderService
            await CreateEmployeeEarningsForNewServices(order, cancellationToken);

            // Обновление EmployeeEarning для измененных OrderService
            await UpdateEmployeeEarningsForUpdatedServices(order, cancellationToken);

            await repositoryManager.SaveAsync(cancellationToken);
        }
        finally
        {
            await repositoryManager.OrderRepository.UnLockItem(request.OrderDto.Id);
        }

        return "";
    }

    private async Task SyncOrderServices(Order order, List<OrderService> incomingServices, List<OrderServiceDto> incomingServiceDtos, CancellationToken cancellationToken)
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
            // Находим соответствующий DTO для получения данных о процентах
            var incomingDto = incomingServiceDtos.FirstOrDefault(dto => 
                (dto.Id == incoming.Id && dto.Id != 0) || 
                (dto.Id == 0 && dto.ServiceId == incoming.ServiceId));

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
                existing.IsAlmazService = incoming.IsAlmazService;

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
                if (needsEarningUpdate && incomingDto != null)
                {
                    _updatedOrderServices.Add((existing.Id, incomingDto));
                }
            }
            else
            {
                // Add new service
                incoming.OrderId = order.Id;
                incoming.Order = order;
                incoming.Service = services.First(s => s.Id == incoming.ServiceId);
                order.OrderServices.Add(incoming);
                
                if (incomingDto != null)
                {
                    _newOrderServices.Add((incoming, incomingDto));
                }
            }
        }
    }

    private Task CreateEmployeeEarningsForNewServices(Order order, CancellationToken cancellationToken)
    {
        if (_newOrderServices.Count == 0) return Task.CompletedTask;

        foreach (var (orderService, dto) in _newOrderServices)
        {
            var createEmployeeEarningDto = new CreateEmployeeEarningDto()
            {
                PrecentForMultipleEmployeers = dto.PrecentForMultipleEmployeers,
                PrecentLaterOrderForEmployee = dto.PrecentLaterOrderForEmployee,
                PrecentLaterOrderForMultipleEmployeers = dto.PrecentLaterOrderForMultipleEmployeers,
                StandartPrecentForEmployee = dto.StandartPrecentForEmployee,
                OrderServiceEmployeesCount = orderService.OrderServiceEmployees.Count,
                OrderStartDateTime = order.DateStart,
                TotalPrice = orderService.PriceTotal,
            };

            orderService.EmployeeEarinig = EmployeeEarinig.CreateEmployeeEarning(createEmployeeEarningDto);
        }

        return Task.CompletedTask;
    }

    private async Task UpdateEmployeeEarningsForUpdatedServices(Order order, CancellationToken cancellationToken)
    {
        if (_updatedOrderServices.Count == 0) return;

        var orderServiceIds = _updatedOrderServices.Select(x => x.orderServiceId).ToList();

        // Загружаем обновленные OrderService с необходимыми данными
        var updatedOrderServices = await repositoryManager.OrderServiceRepository
            .FindByCondition(os => orderServiceIds.Contains(os.Id), true)
            .Include(os => os.OrderServiceEmployees)
            .Include(os => os.EmployeeEarinig)
            .ToListAsync(cancellationToken);

        foreach (var orderService in updatedOrderServices)
        {
            // Находим соответствующий DTO
            var dto = _updatedOrderServices.First(x => x.orderServiceId == orderService.Id).dto;

            // Удаляем старый EmployeeEarning если существует
            if (orderService.EmployeeEarinig != null)
            {
                repositoryManager.EmployeeEarningRepository.Delete(orderService.EmployeeEarinig);
            }

            // Создаем новый EmployeeEarning с обновленными данными
            var createEmployeeEarningDto = new CreateEmployeeEarningDto()
            {
                PrecentForMultipleEmployeers = dto.PrecentForMultipleEmployeers,
                PrecentLaterOrderForEmployee = dto.PrecentLaterOrderForEmployee,
                PrecentLaterOrderForMultipleEmployeers = dto.PrecentLaterOrderForMultipleEmployeers,
                StandartPrecentForEmployee = dto.StandartPrecentForEmployee,
                OrderServiceEmployeesCount = orderService.OrderServiceEmployees.Count,
                OrderStartDateTime = order.DateStart,
                TotalPrice = orderService.PriceTotal,
            };

            orderService.EmployeeEarinig = EmployeeEarinig.CreateEmployeeEarning(createEmployeeEarningDto);
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
