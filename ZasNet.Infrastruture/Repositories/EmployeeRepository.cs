using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class EmployeeRepository(ZasNetDbContext zasNetDbContext)
    : LockedItemRepository<Employee>(zasNetDbContext), IEmployeeRepository 
{
}
