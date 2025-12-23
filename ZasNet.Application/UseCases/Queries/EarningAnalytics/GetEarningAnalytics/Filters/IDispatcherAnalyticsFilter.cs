using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Базовый интерфейс фильтра для аналитики по диспетчерам
/// </summary>
public interface IDispatcherAnalyticsFilter
{
    IQueryable<DispetcherEarning> Accept(IDispatcherAnalyticsFilterVisitor visitor, IQueryable<DispetcherEarning> query);
}

