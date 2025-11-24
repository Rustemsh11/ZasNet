using ZasNet.Domain.Entities;

namespace ZasNet.Application.Repository;

public interface ILockedItemRepository<T> : IRepository<T> where T : LockedItemBase
{
    Task<int?> IsLockedBy(int id);

    Task LockItem(int id, int employeeId);

    Task UnLockItem(int id);
}
