using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Фильтр по клиенту заявки
/// </summary>
public class ClientFilter : IDispetcherEarningFilter
{
    public string ClientSearchTerm { get; set; } = string.Empty;

    public IQueryable<DispetcherEarning> Accept(IDispetcherEarningFilterVisitor visitor, IQueryable<DispetcherEarning> query)
    {
        return visitor.Visit(this, query);
    }
}

