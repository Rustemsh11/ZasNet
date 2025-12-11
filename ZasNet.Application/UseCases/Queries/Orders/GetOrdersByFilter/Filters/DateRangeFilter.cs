using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Фильтр по диапазону дат
/// </summary>
public class DateRangeFilter : IOrderFilter
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public IQueryable<Order> Accept(IOrderFilterVisitor visitor, IQueryable<Order> query)
    {
        return visitor.Visit(this, query);
    }
}

