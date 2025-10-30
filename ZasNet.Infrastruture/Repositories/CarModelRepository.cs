using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class CarModelRepository(ZasNetDbContext zasNetDbContext) 
    : Repository<CarModel>(zasNetDbContext), ICarModelRepository
{
}
