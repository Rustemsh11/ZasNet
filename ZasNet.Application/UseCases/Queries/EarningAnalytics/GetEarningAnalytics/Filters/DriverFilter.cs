using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Фильтр по водителям
/// </summary>
public class DriverFilter : IDriverAnalyticsFilter
{
    public List<int>? DriverIds { get; set; }

    public IQueryable<OrderServiceEmployee> Accept(IDriverAnalyticsFilterVisitor visitor, IQueryable<OrderServiceEmployee> query)
    {
        return visitor.Visit(this, query);
    }
}

