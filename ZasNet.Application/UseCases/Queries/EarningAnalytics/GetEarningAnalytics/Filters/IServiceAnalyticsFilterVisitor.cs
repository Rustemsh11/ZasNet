using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Интерфейс Visitor для применения фильтров к аналитике по услугам
/// </summary>
public interface IServiceAnalyticsFilterVisitor
{
    IQueryable<OrderService> Visit(DateRangeFilter filter, IQueryable<OrderService> query);
    IQueryable<OrderService> Visit(ServiceFilter filter, IQueryable<OrderService> query);
}

