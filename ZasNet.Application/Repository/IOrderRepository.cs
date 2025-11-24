using ZasNet.Domain.Entities;

namespace ZasNet.Application.Repository;

public interface IOrderRepository: ILockedItemRepository<Order>
{
}
