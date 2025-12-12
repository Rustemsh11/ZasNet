using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Интерфейс Visitor для применения фильтров к запросам EmployeeEarning
/// </summary>
public interface IEmployeeEarningFilterVisitor
{
    IQueryable<EmployeeEarinig> Visit(MonthYearFilter filter, IQueryable<EmployeeEarinig> query);
    IQueryable<EmployeeEarinig> Visit(EmployeeFilter filter, IQueryable<EmployeeEarinig> query);
    IQueryable<EmployeeEarinig> Visit(ClientFilter filter, IQueryable<EmployeeEarinig> query);
    IQueryable<EmployeeEarinig> Visit(OrderDateFilter filter, IQueryable<EmployeeEarinig> query);
    IQueryable<EmployeeEarinig> Visit(ServiceFilter filter, IQueryable<EmployeeEarinig> query);
}

