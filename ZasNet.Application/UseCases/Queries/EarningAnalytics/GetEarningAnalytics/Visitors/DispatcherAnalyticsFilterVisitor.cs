using ZasNet.Domain.Entities;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

/// <summary>
/// Реализация Visitor для применения фильтров к аналитике по диспетчерам
/// </summary>
public class DispatcherAnalyticsFilterVisitor : IDispatcherAnalyticsFilterVisitor
{
    public IQueryable<DispetcherEarning> Visit(DateRangeFilter filter, IQueryable<DispetcherEarning> query)
    {
        query = query.Where(de => de.Order.DateStart >= filter.DateFrom
                               && de.Order.DateStart <= filter.DateTo);
        return query;
    }

    public IQueryable<DispetcherEarning> Visit(DispatcherFilter filter, IQueryable<DispetcherEarning> query)
    {
        if (filter.DispatcherIds != null && filter.DispatcherIds.Any())
        {
            query = query.Where(de => filter.DispatcherIds.Contains(de.Order.CreatedEmployeeId));
        }
        return query;
    }
}

