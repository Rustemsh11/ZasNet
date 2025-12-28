using ZasNet.Domain.Entities;

namespace ZasNet.Application.Services.Telegram;

public interface IOrderNotificationService
{
    Task NotifyOrderCreatedAsync(Order order, long chatId, CancellationToken cancellationToken);

    Task NotifyOrderCreatedAsync(long chatId, CancellationToken cancellationToken);
}
