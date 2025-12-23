using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDriverEarningAnalytics;

/// <summary>
/// Обработчик запроса на получение аналитики заработков по водителям
/// </summary>
public class GetDriverEarningAnalyticsHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetDriverEarningAnalyticsRequest, List<DriverEarningAnalyticsDto>>
{
    public async Task<List<DriverEarningAnalyticsDto>> Handle(
        GetDriverEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        // Начинаем с базового запроса с необходимыми включениями
        // Убираем циклический Include OrderServiceEmployees, чтобы избежать цикла в no-tracking запросах
        IQueryable<ZasNet.Domain.Entities.OrderServiceEmployee> query = repositoryManager.OrderEmployeeRepository.FindAll(false)
            .Include(ose => ose.Employee)
            .Include(ose => ose.OrderService)
                .ThenInclude(os => os.Order);

        // Создаем список фильтров на основе запроса
        var filters = BuildFilters(request);

        // Применяем все фильтры через паттерн Visitor
        var visitor = new DriverAnalyticsFilterVisitor();
        foreach (var filter in filters)
        {
            query = filter.Accept(visitor, query);
        }

        var driverData = await query.ToListAsync(cancellationToken);

        // Загружаем OrderServiceEmployees для каждого OrderService отдельно, чтобы избежать циклического Include
        var orderServiceIds = driverData.Select(ose => ose.OrderServiceId).Distinct().ToList();
        var orderServiceEmployeesMap = await repositoryManager.OrderEmployeeRepository
            .FindAll(false)
            .Where(ose => orderServiceIds.Contains(ose.OrderServiceId))
            .GroupBy(ose => ose.OrderServiceId)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

        // Группируем по водителям и распределяем прибыль пропорционально
        // Используем PriceTotal для расчета того, сколько рублей принес каждый сотрудник для компании
        var analytics = driverData
            .GroupBy(ose => new { ose.EmployeeId, ose.Employee.Name })
            .Select(g => new DriverEarningAnalyticsDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = g.Key.Name,
                TotalZasNetEarningByEmployee = g
                    .GroupBy(ose => ose.OrderServiceId)
                    .Sum(group =>
                    {
                        var orderService = group.First().OrderService;
                        var totalEmployees = orderServiceEmployeesMap.GetValueOrDefault(orderService.Id, 1);
                        // Если одну услугу выполняли несколько сотрудников, делим сумму поровну на всех
                        return totalEmployees > 0
                            ? orderService.PriceTotal / totalEmployees
                            : 0;
                    }),
                OrdersCount = g.Select(ose => ose.OrderService.OrderId).Distinct().Count(),
            })
            .ToList();

        return analytics;
    }

    /// <summary>
    /// Создает список фильтров для аналитики по водителям на основе параметров запроса
    /// </summary>
    private List<IDriverAnalyticsFilter> BuildFilters(GetDriverEarningAnalyticsRequest request)
    {
        var filters = new List<IDriverAnalyticsFilter>();

        // Обязательный фильтр по диапазону дат
        filters.Add(new DateRangeFilter
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo
        });

        // Фильтр по водителям
        if (request.DriverIds != null && request.DriverIds.Any())
        {
            filters.Add(new DriverFilter
            {
                DriverIds = request.DriverIds
            });
        }

        return filters;
    }
}

