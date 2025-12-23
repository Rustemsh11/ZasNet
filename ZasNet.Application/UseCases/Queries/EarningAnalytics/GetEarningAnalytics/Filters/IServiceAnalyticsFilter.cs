using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Базовый интерфейс фильтра для аналитики по услугам
/// </summary>
public interface IServiceAnalyticsFilter
{
    IQueryable<OrderService> Accept(IServiceAnalyticsFilterVisitor visitor, IQueryable<OrderService> query);
}

