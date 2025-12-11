using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Интерфейс Visitor для применения фильтров к запросам заявок
/// </summary>
public interface IOrderFilterVisitor
{
    IQueryable<Order> Visit(DateRangeFilter filter, IQueryable<Order> query);
    IQueryable<Order> Visit(StatusFilter filter, IQueryable<Order> query);
    IQueryable<Order> Visit(ClientSearchFilter filter, IQueryable<Order> query);
    IQueryable<Order> Visit(PaymentTypeFilter filter, IQueryable<Order> query);
    IQueryable<Order> Visit(ServiceFilter filter, IQueryable<Order> query);
    IQueryable<Order> Visit(CreatedEmployeeFilter filter, IQueryable<Order> query);
}

