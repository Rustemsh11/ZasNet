using ZasNet.Domain.Entities;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

/// <summary>
/// Реализация Visitor для применения фильтров к аналитике по услугам
/// </summary>
public class ServiceAnalyticsFilterVisitor : IServiceAnalyticsFilterVisitor
{
    public IQueryable<OrderService> Visit(DateRangeFilter filter, IQueryable<OrderService> query)
    {
        query = query.Where(os => os.Order.DateStart >= filter.DateFrom
                                && os.Order.DateStart <= filter.DateTo);
        return query;
    }

    public IQueryable<OrderService> Visit(ServiceFilter filter, IQueryable<OrderService> query)
    {
        if (filter.ServiceIds != null && filter.ServiceIds.Any())
        {
            query = query.Where(os => filter.ServiceIds.Contains(os.ServiceId));
        }
        return query;
    }
}

