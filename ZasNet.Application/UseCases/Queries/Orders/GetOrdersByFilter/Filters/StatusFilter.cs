using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Фильтр по статусам заявок
/// </summary>
public class StatusFilter : IOrderFilter
{
    public List<OrderStatus> Statuses { get; set; } = new();

    public IQueryable<Order> Accept(IOrderFilterVisitor visitor, IQueryable<Order> query)
    {
        return visitor.Visit(this, query);
    }
}

