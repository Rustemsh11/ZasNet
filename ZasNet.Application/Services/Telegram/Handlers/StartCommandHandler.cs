using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class StartCommandHandler : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        throw new NotImplementedException();
    }

    public Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
