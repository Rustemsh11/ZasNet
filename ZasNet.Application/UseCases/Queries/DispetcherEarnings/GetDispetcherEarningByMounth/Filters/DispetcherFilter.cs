using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Фильтр по диспетчерам
/// </summary>
public class DispetcherFilter : IDispetcherEarningFilter
{
    public List<int> DispetcherIds { get; set; } = new();

    public IQueryable<DispetcherEarning> Accept(IDispetcherEarningFilterVisitor visitor, IQueryable<DispetcherEarning> query)
    {
        return visitor.Visit(this, query);
    }
}

