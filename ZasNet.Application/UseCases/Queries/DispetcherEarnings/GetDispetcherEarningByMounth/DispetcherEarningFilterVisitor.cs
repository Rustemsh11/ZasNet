using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Реализация Visitor для применения фильтров к запросам DispetcherEarning
/// </summary>
public class DispetcherEarningFilterVisitor : IDispetcherEarningFilterVisitor
{
    public IQueryable<DispetcherEarning> Visit(MonthYearFilter filter, IQueryable<DispetcherEarning> query)
    {
        // Фильтруем по месяцу и году через дату заявки
        var startDate = new DateTime(filter.Year, filter.Month, 1);
        var endDate = startDate.AddMonths(1);

        query = query.Where(de => de.Order.DateStart >= startDate 
                                && de.Order.DateStart < endDate);

        return query;
    }

    public IQueryable<DispetcherEarning> Visit(DispetcherFilter filter, IQueryable<DispetcherEarning> query)
    {
        if (filter.DispetcherIds != null && filter.DispetcherIds.Any())
        {
            // Фильтруем по диспетчеру (создателю заявки)
            query = query.Where(de => filter.DispetcherIds.Contains(de.Order.CreatedEmployeeId));
        }

        return query;
    }

    public IQueryable<DispetcherEarning> Visit(ClientFilter filter, IQueryable<DispetcherEarning> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.ClientSearchTerm))
        {
            query = query.Where(de => EF.Functions.Like(de.Order.Client, $"%{filter.ClientSearchTerm}%"));
        }

        return query;
    }

    public IQueryable<DispetcherEarning> Visit(OrderDateFilter filter, IQueryable<DispetcherEarning> query)
    {
        if (filter.DateFrom.HasValue)
        {
            query = query.Where(de => de.Order.DateStart >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(de => de.Order.DateEnd <= filter.DateTo.Value);
        }

        return query;
    }
}

