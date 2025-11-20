using ZasNet.Application.Services;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Services.Telegram;

public class NoOpOrderNotificationService : IOrderNotificationService
{
    public Task NotifyOrderCreatedAsync(Order order, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

