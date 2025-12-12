using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Фильтр по сотрудникам
/// </summary>
public class EmployeeFilter : IEmployeeEarningFilter
{
    public List<int> EmployeeIds { get; set; } = new();

    public IQueryable<EmployeeEarinig> Accept(IEmployeeEarningFilterVisitor visitor, IQueryable<EmployeeEarinig> query)
    {
        return visitor.Visit(this, query);
    }
}

