using ZasNet.Domain.Entities;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

/// <summary>
/// Реализация Visitor для применения фильтров к аналитике по водителям
/// </summary>
public class DriverAnalyticsFilterVisitor : IDriverAnalyticsFilterVisitor
{
    public IQueryable<OrderServiceEmployee> Visit(DateRangeFilter filter, IQueryable<OrderServiceEmployee> query)
    {
        query = query.Where(ose => ose.OrderService.Order.DateStart >= filter.DateFrom
                                && ose.OrderService.Order.DateStart <= filter.DateTo);
        return query;
    }

    public IQueryable<OrderServiceEmployee> Visit(DriverFilter filter, IQueryable<OrderServiceEmployee> query)
    {
        if (filter.DriverIds != null && filter.DriverIds.Any())
        {
            query = query.Where(ose => filter.DriverIds.Contains(ose.EmployeeId));
        }
        return query;
    }
}

