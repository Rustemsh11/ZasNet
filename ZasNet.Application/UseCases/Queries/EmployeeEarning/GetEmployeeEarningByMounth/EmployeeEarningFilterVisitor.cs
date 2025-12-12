using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Реализация Visitor для применения фильтров к запросам EmployeeEarning
/// </summary>
public class EmployeeEarningFilterVisitor : IEmployeeEarningFilterVisitor
{
    public IQueryable<EmployeeEarinig> Visit(MonthYearFilter filter, IQueryable<EmployeeEarinig> query)
    {
        // Фильтруем по месяцу и году через дату заявки
        var startDate = new DateTime(filter.Year, filter.Month, 1);
        var endDate = startDate.AddMonths(1);

        query = query.Where(ee => ee.OrderService.Order.DateStart >= startDate 
                                && ee.OrderService.Order.DateStart < endDate);

        return query;
    }

    public IQueryable<EmployeeEarinig> Visit(EmployeeFilter filter, IQueryable<EmployeeEarinig> query)
    {
        if (filter.EmployeeIds != null && filter.EmployeeIds.Any())
        {
            // Фильтруем по сотрудникам, назначенным на OrderService
            query = query.Where(ee => ee.OrderService.OrderServiceEmployees
                .Any(ose => filter.EmployeeIds.Contains(ose.EmployeeId)));
        }

        return query;
    }

    public IQueryable<EmployeeEarinig> Visit(ClientFilter filter, IQueryable<EmployeeEarinig> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.ClientSearchTerm))
        {
            query = query.Where(ee => EF.Functions.Like(ee.OrderService.Order.Client, $"%{filter.ClientSearchTerm}%"));
        }

        return query;
    }

    public IQueryable<EmployeeEarinig> Visit(OrderDateFilter filter, IQueryable<EmployeeEarinig> query)
    {
        if (filter.DateFrom.HasValue)
        {
            query = query.Where(ee => ee.OrderService.Order.DateStart >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(ee => ee.OrderService.Order.DateEnd <= filter.DateTo.Value);
        }

        return query;
    }

    public IQueryable<EmployeeEarinig> Visit(ServiceFilter filter, IQueryable<EmployeeEarinig> query)
    {
        if (filter.ServiceIds != null && filter.ServiceIds.Any())
        {
            query = query.Where(ee => filter.ServiceIds.Contains(ee.OrderService.ServiceId));
        }

        return query;
    }
}

