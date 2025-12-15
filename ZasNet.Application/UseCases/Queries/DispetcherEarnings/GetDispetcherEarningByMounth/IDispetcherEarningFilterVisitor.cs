using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Интерфейс Visitor для применения фильтров к запросам DispetcherEarning
/// </summary>
public interface IDispetcherEarningFilterVisitor
{
    IQueryable<DispetcherEarning> Visit(MonthYearFilter filter, IQueryable<DispetcherEarning> query);
    IQueryable<DispetcherEarning> Visit(DispetcherFilter filter, IQueryable<DispetcherEarning> query);
    IQueryable<DispetcherEarning> Visit(ClientFilter filter, IQueryable<DispetcherEarning> query);
    IQueryable<DispetcherEarning> Visit(OrderDateFilter filter, IQueryable<DispetcherEarning> query);
}

