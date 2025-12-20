using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class Repository<T> : IRepository<T> where T : BaseItem
{
    protected readonly ZasNetDbContext zasNetDbContext;

    public Repository(ZasNetDbContext zasNetDbContext)
    {
        this.zasNetDbContext = zasNetDbContext;
    }

    public void Create(T entity)
    {
        zasNetDbContext.Set<T>().Add(entity);
    }

    public void Delete(T entity)
    {
        zasNetDbContext.Set<T>().Remove(entity);
    }

    public IQueryable<T> FindAll(bool trackChanges)
    {
        if (trackChanges) 
        {
            return zasNetDbContext.Set<T>().Where(c=>c.IsDeleted != true);
        }

        return zasNetDbContext.Set<T>().Where(c => c.IsDeleted != true).AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
    {
        if (trackChanges)
        {
            return zasNetDbContext.Set<T>().Where(c => c.IsDeleted != true).Where(expression);
        }

        return zasNetDbContext.Set<T>().Where(c => c.IsDeleted != true).Where(expression).AsNoTracking();
    }

    public void Update(T entity)
    {
        zasNetDbContext.Set<T>().Update(entity);
    }
}
