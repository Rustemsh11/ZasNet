using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetServiceEarningAnalytics;

/// <summary>
/// Обработчик запроса на получение аналитики заработков по услугам
/// </summary>
public class GetServiceEarningAnalyticsHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetServiceEarningAnalyticsRequest, List<ServiceEarningAnalyticsDto>>
{
    public async Task<List<ServiceEarningAnalyticsDto>> Handle(
        GetServiceEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        // Начинаем с базового запроса с необходимыми включениями
        IQueryable<ZasNet.Domain.Entities.OrderService> query = repositoryManager.OrderServiceRepository.FindAll(false)
            .Include(os => os.Service)
            .Include(os => os.EmployeeEarinig)
            .Include(os => os.Order);

        // Создаем список фильтров на основе запроса
        var filters = BuildFilters(request);

        // Применяем все фильтры через паттерн Visitor
        var visitor = new ServiceAnalyticsFilterVisitor();
        foreach (IServiceAnalyticsFilter filter in filters)
        {
            query = filter.Accept(visitor, query);
        }

        var serviceData = await query.ToListAsync(cancellationToken);

        // Группируем по услугам
        var analytics = serviceData
            .GroupBy(os => new { os.ServiceId, os.Service.Name })
            .Select(g => new ServiceEarningAnalyticsDto
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.Name,
                TotalEmployeesEarnings = g
                    .Where(os => os.EmployeeEarinig != null)
                    .Sum(os => os.EmployeeEarinig.EmployeeEarning),
                OrdersCount = g.Select(os => os.OrderId).Distinct().Count(),
                ServicesCount = g.Count(),
                TotalZasNetEarning = g.Sum(os => os.PriceTotal)
            })
            .ToList();

        return analytics;
    }

    /// <summary>
    /// Создает список фильтров для аналитики по услугам на основе параметров запроса
    /// </summary>
    private List<IServiceAnalyticsFilter> BuildFilters(GetServiceEarningAnalyticsRequest request)
    {
        var filters = new List<IServiceAnalyticsFilter>();

        // Обязательный фильтр по диапазону дат
        filters.Add(new DateRangeFilter
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo
        });

        // Фильтр по услугам
        if (request.ServiceIds != null && request.ServiceIds.Any())
        {
            filters.Add(new ServiceFilter
            {
                ServiceIds = request.ServiceIds
            });
        }

        return filters;
    }
}

