using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Фильтр по дате заявки
/// </summary>
public class OrderDateFilter : IDispetcherEarningFilter
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public IQueryable<DispetcherEarning> Accept(IDispetcherEarningFilterVisitor visitor, IQueryable<DispetcherEarning> query)
    {
        return visitor.Visit(this, query);
    }
}

