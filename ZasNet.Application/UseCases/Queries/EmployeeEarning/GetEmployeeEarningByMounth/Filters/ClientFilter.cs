using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Фильтр по клиенту заявки
/// </summary>
public class ClientFilter : IEmployeeEarningFilter
{
    public string ClientSearchTerm { get; set; } = string.Empty;

    public IQueryable<EmployeeEarinig> Accept(IEmployeeEarningFilterVisitor visitor, IQueryable<EmployeeEarinig> query)
    {
        return visitor.Visit(this, query);
    }
}

