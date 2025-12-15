using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class DispetcherEarningRepository(ZasNetDbContext zasNetDbContext)
    : Repository<DispetcherEarning>(zasNetDbContext), IDispetcherEarningRepository
{
}

