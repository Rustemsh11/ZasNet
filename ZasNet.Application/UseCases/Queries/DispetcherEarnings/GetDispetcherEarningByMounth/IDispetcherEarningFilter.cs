using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Базовый интерфейс фильтра для DispetcherEarning
/// </summary>
public interface IDispetcherEarningFilter
{
    IQueryable<DispetcherEarning> Accept(IDispetcherEarningFilterVisitor visitor, IQueryable<DispetcherEarning> query);
}

