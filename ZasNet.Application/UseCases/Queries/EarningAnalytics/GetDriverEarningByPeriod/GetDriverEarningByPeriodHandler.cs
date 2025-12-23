using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDriverEarningByPeriod;

/// <summary>
/// Обработчик запроса на получение прибыли водителей по периодам
/// </summary>
public class GetDriverEarningByPeriodHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetDriverEarningByPeriodRequest, List<DriverEarningByPeriodDto>>
{
    public async Task<List<DriverEarningByPeriodDto>> Handle(
        GetDriverEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        // Базовый запрос для получения данных о водителях и заявках
        var query = repositoryManager.OrderEmployeeRepository
            .FindAll(false)
            .Include(ose => ose.Employee)
            .Include(ose => ose.OrderService)
                .ThenInclude(os => os.Order)
            .Where(ose => ose.OrderService.Order.DateStart >= request.DateFrom
                       && ose.OrderService.Order.DateStart <= request.DateTo);

        if (request.DriverIds != null && request.DriverIds.Any())
        {
            query = query.Where(ose => request.DriverIds.Contains(ose.EmployeeId));
        }

        var orderServiceEmployees = await query.ToListAsync(cancellationToken);

        // Загружаем количество сотрудников для каждого OrderService для правильного распределения прибыли
        var orderServiceIds = orderServiceEmployees.Select(ose => ose.OrderServiceId).Distinct().ToList();
        var orderServiceEmployeesCount = await repositoryManager.OrderEmployeeRepository
            .FindAll(false)
            .Where(ose => orderServiceIds.Contains(ose.OrderServiceId))
            .GroupBy(ose => ose.OrderServiceId)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

        // Группировка в зависимости от выбранного типа периода
        return request.GroupPeriod switch
        {
            GroupPeriod.Day => GetDailyEarnings(orderServiceEmployees, orderServiceEmployeesCount),
            GroupPeriod.Month => GetMonthlyEarnings(orderServiceEmployees, orderServiceEmployeesCount),
            GroupPeriod.Year => GetYearlyEarnings(orderServiceEmployees, orderServiceEmployeesCount),
            _ => throw new ArgumentException($"Неизвестный тип периода: {request.GroupPeriod}")
        };
    }

    /// <summary>
    /// Группировка по дням
    /// </summary>
    private List<DriverEarningByPeriodDto> GetDailyEarnings(
        List<OrderServiceEmployee> orderServiceEmployees,
        Dictionary<int, int> orderServiceEmployeesCount)
    {
        return orderServiceEmployees
            .GroupBy(ose => new
            {
                Date = ose.OrderService.Order.DateStart.Date,
                DriverId = ose.EmployeeId,
                DriverName = ose.Employee.Name
            })
            .Select(g => new DriverEarningByPeriodDto
            {
                Period = g.Key.Date,
                GroupPeriod = GroupPeriod.Day,
                DriverId = g.Key.DriverId,
                DriverName = g.Key.DriverName,
                TotalEarning = g
                    .GroupBy(ose => ose.OrderServiceId)
                    .Sum(group =>
                    {
                        var orderService = group.First().OrderService;
                        var totalEmployees = orderServiceEmployeesCount.GetValueOrDefault(orderService.Id, 1);
                        // Если одну услугу выполняли несколько сотрудников, делим сумму поровну на всех
                        return totalEmployees > 0
                            ? orderService.PriceTotal / totalEmployees
                            : 0;
                    })
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.DriverName)
            .ToList();
    }

    /// <summary>
    /// Группировка по месяцам
    /// </summary>
    private List<DriverEarningByPeriodDto> GetMonthlyEarnings(
        List<OrderServiceEmployee> orderServiceEmployees,
        Dictionary<int, int> orderServiceEmployeesCount)
    {
        return orderServiceEmployees
            .GroupBy(ose => new
            {
                Year = ose.OrderService.Order.DateStart.Year,
                Month = ose.OrderService.Order.DateStart.Month,
                FirstDayOfMonth = new DateTime(ose.OrderService.Order.DateStart.Year, ose.OrderService.Order.DateStart.Month, 1),
                DriverId = ose.EmployeeId,
                DriverName = ose.Employee.Name
            })
            .Select(g => new DriverEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfMonth,
                GroupPeriod = GroupPeriod.Month,
                DriverId = g.Key.DriverId,
                DriverName = g.Key.DriverName,
                TotalEarning = g
                    .GroupBy(ose => ose.OrderServiceId)
                    .Sum(group =>
                    {
                        var orderService = group.First().OrderService;
                        var totalEmployees = orderServiceEmployeesCount.GetValueOrDefault(orderService.Id, 1);
                        // Если одну услугу выполняли несколько сотрудников, делим сумму поровну на всех
                        return totalEmployees > 0
                            ? orderService.PriceTotal / totalEmployees
                            : 0;
                    })
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.DriverName)
            .ToList();
    }

    /// <summary>
    /// Группировка по годам
    /// </summary>
    private List<DriverEarningByPeriodDto> GetYearlyEarnings(
        List<OrderServiceEmployee> orderServiceEmployees,
        Dictionary<int, int> orderServiceEmployeesCount)
    {
        return orderServiceEmployees
            .GroupBy(ose => new
            {
                Year = ose.OrderService.Order.DateStart.Year,
                FirstDayOfYear = new DateTime(ose.OrderService.Order.DateStart.Year, 1, 1),
                DriverId = ose.EmployeeId,
                DriverName = ose.Employee.Name
            })
            .Select(g => new DriverEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfYear,
                GroupPeriod = GroupPeriod.Year,
                DriverId = g.Key.DriverId,
                DriverName = g.Key.DriverName,
                TotalEarning = g
                    .GroupBy(ose => ose.OrderServiceId)
                    .Sum(group =>
                    {
                        var orderService = group.First().OrderService;
                        var totalEmployees = orderServiceEmployeesCount.GetValueOrDefault(orderService.Id, 1);
                        // Если одну услугу выполняли несколько сотрудников, делим сумму поровну на всех
                        return totalEmployees > 0
                            ? orderService.PriceTotal / totalEmployees
                            : 0;
                    })
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.DriverName)
            .ToList();
    }
}

