using ZasNet.Domain.Entities;

namespace ZasNet.Application.Services;

public interface IOrderNotificationService
{
    Task NotifyOrderCreatedAsync(Order order, CancellationToken cancellationToken);
}

