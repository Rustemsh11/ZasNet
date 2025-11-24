using ZasNet.Domain.Entities;

namespace ZasNet.Application.Services.Telegram;

public class NoOpOrderNotificationService : IOrderNotificationService
{
    public Task NotifyOrderCreatedAsync(Order order, long chatId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
