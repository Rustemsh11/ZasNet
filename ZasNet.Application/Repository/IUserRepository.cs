using System.Linq.Expressions;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.Repository;

public interface IUserRepository
{
    IQueryable<User> FindByCondition(Expression<Func<User, bool>> expression, bool trackChanges);

    void Create(User user);

    void Update(User user);

}
