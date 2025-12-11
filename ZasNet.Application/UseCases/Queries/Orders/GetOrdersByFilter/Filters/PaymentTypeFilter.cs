using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Фильтр по типам оплаты
/// </summary>
public class PaymentTypeFilter : IOrderFilter
{
    public List<PaymentType> PaymentTypes { get; set; } = new();

    public IQueryable<Order> Accept(IOrderFilterVisitor visitor, IQueryable<Order> query)
    {
        return visitor.Visit(this, query);
    }
}

