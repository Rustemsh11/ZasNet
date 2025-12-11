using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Фильтр поиска по названию клиента
/// </summary>
public class ClientSearchFilter : IOrderFilter
{
    public string? SearchTerm { get; set; }

    public IQueryable<Order> Accept(IOrderFilterVisitor visitor, IQueryable<Order> query)
    {
        return visitor.Visit(this, query);
    }
}

