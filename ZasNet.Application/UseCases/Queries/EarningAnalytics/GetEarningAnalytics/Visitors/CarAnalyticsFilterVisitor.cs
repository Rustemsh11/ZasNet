using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Entities;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

/// <summary>
/// Реализация Visitor для применения фильтров к аналитике по машинам
/// </summary>
public class CarAnalyticsFilterVisitor : ICarAnalyticsFilterVisitor
{
    public IQueryable<OrderServiceCar> Visit(DateRangeFilter filter, IQueryable<OrderServiceCar> query)
    {
        query = query.Where(osc => osc.OrderService.Order.DateStart >= filter.DateFrom
                                 && osc.OrderService.Order.DateStart <= filter.DateTo);
        return query;
    }

    public IQueryable<OrderServiceCar> Visit(CarFilter filter, IQueryable<OrderServiceCar> query)
    {
        if (filter.CarIds != null && filter.CarIds.Any())
        {
            query = query.Where(osc => filter.CarIds.Contains(osc.CarId));
        }
        return query;
    }
}

