using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Интерфейс Visitor для применения фильтров к аналитике по диспетчерам
/// </summary>
public interface IDispatcherAnalyticsFilterVisitor
{
    IQueryable<DispetcherEarning> Visit(DateRangeFilter filter, IQueryable<DispetcherEarning> query);
    IQueryable<DispetcherEarning> Visit(DispatcherFilter filter, IQueryable<DispetcherEarning> query);
}

