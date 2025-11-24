using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class LockedItemRepository<T> : Repository<T>, ILockedItemRepository<T> where T : LockedItemBase
{
    public LockedItemRepository(ZasNetDbContext zasNetDbContext) 
        : base(zasNetDbContext)
    {
    }

    public async Task<int?> IsLockedBy(int id)
    {
        var isLockedby = await this.FindByCondition(c => c.Id == id && c.LockedByUserId != null, false).Select(c=>c.LockedByUserId).SingleOrDefaultAsync();
        return isLockedby;
    }

    public async Task LockItem(int id, int employeeId)
    {
        var entity = await this.FindByCondition(c => c.Id == id, true).SingleOrDefaultAsync();
        entity.LockedByUserId = employeeId;
        entity.LockedAt = DateTime.Now;

        zasNetDbContext.Update(entity);
        await zasNetDbContext.SaveChangesAsync();
    }

    public async Task UnLockItem(int id)
    {
        var entity = await this.FindByCondition(c => c.Id == id, true).SingleOrDefaultAsync();
        entity.LockedByUserId = null;
        entity.LockedAt = null;

        zasNetDbContext.Update(entity);
        await zasNetDbContext.SaveChangesAsync();
    }
}
