using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Фильтр по месяцу и году
/// </summary>
public class MonthYearFilter : IDispetcherEarningFilter
{
    public int Year { get; set; }
    public int Month { get; set; }

    public IQueryable<DispetcherEarning> Accept(IDispetcherEarningFilterVisitor visitor, IQueryable<DispetcherEarning> query)
    {
        return visitor.Visit(this, query);
    }
}

