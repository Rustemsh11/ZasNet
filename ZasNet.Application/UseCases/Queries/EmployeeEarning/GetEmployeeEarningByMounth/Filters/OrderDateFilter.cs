using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Фильтр по дате заявки
/// </summary>
public class OrderDateFilter : IEmployeeEarningFilter
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public IQueryable<EmployeeEarinig> Accept(IEmployeeEarningFilterVisitor visitor, IQueryable<EmployeeEarinig> query)
    {
        return visitor.Visit(this, query);
    }
}

