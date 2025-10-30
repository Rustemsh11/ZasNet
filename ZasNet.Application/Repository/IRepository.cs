using System.Linq.Expressions;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.Repository;

public interface IRepository<T> where T : BaseItem
{
    IQueryable<T> FindAll(bool trackChanges);

    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);

    void Create(T entity);
    
    void Update(T entity);

    void Delete(T entity);
}
