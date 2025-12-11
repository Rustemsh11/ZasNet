using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Фильтр по сотруднику, создавшему заявку
/// </summary>
public class CreatedEmployeeFilter : IOrderFilter
{
    public List<int> EmployeeIds { get; set; } = new();

    public IQueryable<Order> Accept(IOrderFilterVisitor visitor, IQueryable<Order> query)
    {
        return visitor.Visit(this, query);
    }
}

