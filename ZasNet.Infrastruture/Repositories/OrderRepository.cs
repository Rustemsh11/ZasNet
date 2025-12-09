using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Security;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class OrderRepository(ZasNetDbContext zasNetDbContext)
    : LockedItemRepository<Order>(zasNetDbContext), IOrderRepository
{
    public IQueryable<Order> GetOrders(Expression<Func<Order, bool>> additionalExpression, SecurityOperations securityOperations, bool trakChanges)
    {
        if(securityOperations == SecurityOperations.GeneralLedger)
        {
            return this.FindByCondition(additionalExpression, trakChanges).Where(c => c.Status == Domain.Enums.OrderStatus.CreatingInvoice)
                .Include(c => c.OrderServices).ThenInclude(c => c.Service)
                .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceEmployees).ThenInclude(c => c.Employee)
                .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceCars).ThenInclude(c => c.Car).ThenInclude(c => c.CarModel)
                .Include(c => c.OrderDocuments);
        }

        return this.FindByCondition(additionalExpression, trakChanges)
            .Include(c => c.OrderServices).ThenInclude(c => c.Service)
                .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceEmployees).ThenInclude(c => c.Employee)
                .Include(c => c.OrderServices).ThenInclude(c => c.OrderServiceCars).ThenInclude(c => c.Car).ThenInclude(c => c.CarModel);
    }
}
