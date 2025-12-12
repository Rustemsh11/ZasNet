using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Фильтр по месяцу и году
/// </summary>
public class MonthYearFilter : IEmployeeEarningFilter
{
    public int Year { get; set; }
    public int Month { get; set; }

    public IQueryable<EmployeeEarinig> Accept(IEmployeeEarningFilterVisitor visitor, IQueryable<EmployeeEarinig> query)
    {
        return visitor.Visit(this, query);
    }
}

