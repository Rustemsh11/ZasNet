using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Интерфейс Visitor для применения фильтров к аналитике по водителям
/// </summary>
public interface IDriverAnalyticsFilterVisitor
{
    IQueryable<OrderServiceEmployee> Visit(DateRangeFilter filter, IQueryable<OrderServiceEmployee> query);
    IQueryable<OrderServiceEmployee> Visit(DriverFilter filter, IQueryable<OrderServiceEmployee> query);
}

