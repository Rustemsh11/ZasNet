using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class ServiceRepository(ZasNetDbContext zasNetDbContext)
    : LockedItemRepository<Service>(zasNetDbContext), IServiceRepository
{
}
