using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDispatcherEarningAnalytics;

/// <summary>
/// Обработчик запроса на получение аналитики заработков по диспетчерам
/// </summary>
public class GetDispatcherEarningAnalyticsHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetDispatcherEarningAnalyticsRequest, List<DispatcherEarningAnalyticsDto>>
{
    public async Task<List<DispatcherEarningAnalyticsDto>> Handle(
        GetDispatcherEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        // Начинаем с базового запроса с необходимыми включениями
        IQueryable<ZasNet.Domain.Entities.DispetcherEarning> query = repositoryManager.DispetcherEarningRepository.FindAll(false)
            .Include(de => de.Order)
                .ThenInclude(o => o.CreatedEmployee);

        // Создаем список фильтров на основе запроса
        var filters = BuildFilters(request);

        // Применяем все фильтры через паттерн Visitor
        var visitor = new DispatcherAnalyticsFilterVisitor();
        foreach (var filter in filters)
        {
            query = filter.Accept(visitor, query);
        }

        var dispatcherData = await query.ToListAsync(cancellationToken);

        // Группируем по диспетчерам
        var analytics = dispatcherData
            .GroupBy(de => new { de.Order.CreatedEmployeeId, de.Order.CreatedEmployee.Name })
            .Select(g => new DispatcherEarningAnalyticsDto
            {
                EmployeeId = g.Key.CreatedEmployeeId,
                EmployeeName = g.Key.Name,
                TotalDispetcherEarnings = g.Sum(de => de.EmployeeEarning),
                TotalZasNetEarningFromDispetcher = g.Sum(de => de.Order.OrderPriceAmount),
                OrdersCount = g.Count(),
            })
            .ToList();

        return analytics;
    }

    /// <summary>
    /// Создает список фильтров для аналитики по диспетчерам на основе параметров запроса
    /// </summary>
    private List<IDispatcherAnalyticsFilter> BuildFilters(GetDispatcherEarningAnalyticsRequest request)
    {
        var filters = new List<IDispatcherAnalyticsFilter>();

        // Обязательный фильтр по диапазону дат
        filters.Add(new DateRangeFilter
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo
        });

        // Фильтр по диспетчерам
        if (request.DispatcherIds != null && request.DispatcherIds.Any())
        {
            filters.Add(new DispatcherFilter
            {
                DispatcherIds = request.DispatcherIds
            });
        }

        return filters;
    }
}

