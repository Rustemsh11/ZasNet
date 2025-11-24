using ZasNet.Domain.Entities;
using ZasNet.Domain.Telegram;

namespace ZasNet.Domain.Interfaces;

public interface ITelegramMessageHandler
{
    bool CanHandle(TelegramUpdate telegramUpdate);
    Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken);
}

public class HandlerResult
{
    public bool Success { get; set; }
    public string? ResponseMessage { get; set; }
    public bool AttachMenu { get; set; }

    public List<Order> Orders { get; set; }
}
