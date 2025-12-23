using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Интерфейс Visitor для применения фильтров к аналитике по машинам
/// </summary>
public interface ICarAnalyticsFilterVisitor
{
    IQueryable<OrderServiceCar> Visit(DateRangeFilter filter, IQueryable<OrderServiceCar> query);
    IQueryable<OrderServiceCar> Visit(CarFilter filter, IQueryable<OrderServiceCar> query);
}

