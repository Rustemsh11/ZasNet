using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Фильтр по машинам
/// </summary>
public class CarFilter : ICarAnalyticsFilter
{
    public List<int>? CarIds { get; set; }

    public IQueryable<OrderServiceCar> Accept(ICarAnalyticsFilterVisitor visitor, IQueryable<OrderServiceCar> query)
    {
        return visitor.Visit(this, query);
    }
}

