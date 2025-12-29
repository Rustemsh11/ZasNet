using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Фильтр по водителям, выполнявшим заявку
/// </summary>
public class ExecutedEmployeeFilter : IOrderFilter
{
    public List<int> EmployeeIds { get; set; } = new();

    public IQueryable<Order> Accept(IOrderFilterVisitor visitor, IQueryable<Order> query)
    {
        return visitor.Visit(this, query);
    }
}

