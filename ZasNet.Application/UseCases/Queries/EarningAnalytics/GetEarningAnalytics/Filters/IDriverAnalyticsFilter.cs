using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Базовый интерфейс фильтра для аналитики по водителям
/// </summary>
public interface IDriverAnalyticsFilter
{
    IQueryable<OrderServiceEmployee> Accept(IDriverAnalyticsFilterVisitor visitor, IQueryable<OrderServiceEmployee> query);
}

