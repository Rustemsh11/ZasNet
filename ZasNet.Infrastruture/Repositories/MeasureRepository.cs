using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class MeasureRepository(ZasNetDbContext zasNetDbContext)
    : Repository<Measure>(zasNetDbContext), IMeasureRepository
{
}
