using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class UserRepository(ZasNetDbContext zasNetDbContext) : IUserRepository
{
    public void Create(User user)
    {
        zasNetDbContext.User.Add(user);
    }

    public IQueryable<User> FindByCondition(Expression<Func<User, bool>> expression, bool trackChanges)
    {
        if (trackChanges)
        {
            return zasNetDbContext.User.Where(expression);
        }

        return zasNetDbContext.User.Where(expression).AsNoTracking();
    }

    public void Update(User user)
    {
        zasNetDbContext.User.Update(user);
    }
}
