using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Фильтр по диспетчерам
/// </summary>
public class DispatcherFilter : IDispatcherAnalyticsFilter
{
    public List<int>? DispatcherIds { get; set; }

    public IQueryable<DispetcherEarning> Accept(IDispatcherAnalyticsFilterVisitor visitor, IQueryable<DispetcherEarning> query)
    {
        return visitor.Visit(this, query);
    }
}

