using ZasNet.Domain.Entities;

namespace ZasNet.Application.Repository;

public interface IEmployeeRepository : ILockedItemRepository<Employee>
{
}
