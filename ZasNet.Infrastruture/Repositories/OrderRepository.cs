using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class OrderRepository(ZasNetDbContext zasNetDbContext)
    : LockedItemRepository<Order>(zasNetDbContext), IOrderRepository
{
}
