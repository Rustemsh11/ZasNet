using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Фильтр по услугам
/// </summary>
public class ServiceFilter : IServiceAnalyticsFilter
{
    public List<int>? ServiceIds { get; set; }

    public IQueryable<OrderService> Accept(IServiceAnalyticsFilterVisitor visitor, IQueryable<OrderService> query)
    {
        return visitor.Visit(this, query);
    }
}

