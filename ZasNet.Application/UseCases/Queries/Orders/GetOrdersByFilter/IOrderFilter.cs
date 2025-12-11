using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Базовый интерфейс фильтра заявок
/// </summary>
public interface IOrderFilter
{
    IQueryable<Order> Accept(IOrderFilterVisitor visitor, IQueryable<Order> query);
}

