using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;

/// <summary>
/// Фильтр по диапазону дат (используется для всех типов аналитики)
/// </summary>
public class DateRangeFilter : ICarAnalyticsFilter, IServiceAnalyticsFilter, IDriverAnalyticsFilter, IDispatcherAnalyticsFilter
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public IQueryable<OrderServiceCar> Accept(ICarAnalyticsFilterVisitor visitor, IQueryable<OrderServiceCar> query)
    {
        return visitor.Visit(this, query);
    }

    public IQueryable<OrderService> Accept(IServiceAnalyticsFilterVisitor visitor, IQueryable<OrderService> query)
    {
        return visitor.Visit(this, query);
    }

    public IQueryable<OrderServiceEmployee> Accept(IDriverAnalyticsFilterVisitor visitor, IQueryable<OrderServiceEmployee> query)
    {
        return visitor.Visit(this, query);
    }

    public IQueryable<DispetcherEarning> Accept(IDispatcherAnalyticsFilterVisitor visitor, IQueryable<DispetcherEarning> query)
    {
        return visitor.Visit(this, query);
    }
}

