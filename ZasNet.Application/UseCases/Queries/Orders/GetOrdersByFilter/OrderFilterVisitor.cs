using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Реализация Visitor для применения фильтров к запросам заявок
/// </summary>
public class OrderFilterVisitor : IOrderFilterVisitor
{
    public IQueryable<Order> Visit(DateRangeFilter filter, IQueryable<Order> query)
    {
        if (filter.DateFrom.HasValue)
        {
            query = query.Where(o => o.DateStart >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(o => o.DateEnd <= filter.DateTo.Value);
        }

        return query;
    }

    public IQueryable<Order> Visit(StatusFilter filter, IQueryable<Order> query)
    {
        if (filter.Statuses != null && filter.Statuses.Any())
        {
            query = query.Where(o => filter.Statuses.Contains(o.Status));
        }

        return query;
    }

    public IQueryable<Order> Visit(ClientSearchFilter filter, IQueryable<Order> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(o => EF.Functions.Like(o.Client, $"%{filter.SearchTerm}%"));
        }

        return query;
    }

    public IQueryable<Order> Visit(PaymentTypeFilter filter, IQueryable<Order> query)
    {
        if (filter.PaymentTypes != null && filter.PaymentTypes.Any())
        {
            query = query.Where(o => filter.PaymentTypes.Contains(o.PaymentType));
        }

        return query;
    }

    public IQueryable<Order> Visit(ServiceFilter filter, IQueryable<Order> query)
    {
        if (filter.ServiceIds != null && filter.ServiceIds.Any())
        {
            query = query.Where(o => o.OrderServices.Any(os => filter.ServiceIds.Contains(os.ServiceId)));
        }

        return query;
    }

    public IQueryable<Order> Visit(CreatedEmployeeFilter filter, IQueryable<Order> query)
    {
        if (filter.EmployeeIds != null && filter.EmployeeIds.Any())
        {
            query = query.Where(o => filter.EmployeeIds.Contains(o.CreatedEmployeeId));
        }

        return query;
    }
}

