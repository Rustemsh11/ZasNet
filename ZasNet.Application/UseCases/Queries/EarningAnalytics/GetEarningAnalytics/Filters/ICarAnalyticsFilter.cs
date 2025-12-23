using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Базовый интерфейс фильтра для аналитики по машинам
/// </summary>
public interface ICarAnalyticsFilter
{
    IQueryable<OrderServiceCar> Accept(ICarAnalyticsFilterVisitor visitor, IQueryable<OrderServiceCar> query);
}

