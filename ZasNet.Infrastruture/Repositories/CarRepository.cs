using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class CarRepository(ZasNetDbContext zasNetDbContext) 
    : Repository<Car>(zasNetDbContext), ICarRepository
{
}
