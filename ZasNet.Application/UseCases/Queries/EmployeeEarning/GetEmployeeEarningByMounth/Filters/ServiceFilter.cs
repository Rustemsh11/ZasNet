using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Фильтр по услугам
/// </summary>
public class ServiceFilter : IEmployeeEarningFilter
{
    public List<int> ServiceIds { get; set; } = new();

    public IQueryable<EmployeeEarinig> Accept(IEmployeeEarningFilterVisitor visitor, IQueryable<EmployeeEarinig> query)
    {
        return visitor.Visit(this, query);
    }
}

