using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class OrderEmployeeRepository(ZasNetDbContext zasNetDbContext)
    : Repository<OrderServiceEmployee>(zasNetDbContext), IOrderEmployeeRepository
{
}
