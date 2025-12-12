using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Базовый интерфейс фильтра для EmployeeEarning
/// </summary>
public interface IEmployeeEarningFilter
{
    IQueryable<EmployeeEarinig> Accept(IEmployeeEarningFilterVisitor visitor, IQueryable<EmployeeEarinig> query);
}

