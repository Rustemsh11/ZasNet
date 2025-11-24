using ZasNet.Domain.Entities;

namespace ZasNet.Application.Repository;

public interface IOrderServiceRepository : ILockedItemRepository<OrderService>
{
}
