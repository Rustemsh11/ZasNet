using System.Linq.Expressions;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Security;

namespace ZasNet.Application.Repository;

public interface IOrderRepository: ILockedItemRepository<Order>
{
    IQueryable<Order> GetOrders(Expression<Func<Order, bool>> additionalExpression, SecurityOperations securityOperations, bool trakChanges);
}
