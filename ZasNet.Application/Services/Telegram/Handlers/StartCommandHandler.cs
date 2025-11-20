using ZasNet.Application.Repository;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class StartCommandHandler : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        return telegramUpdate?.Message?.Text == "/start";
    }

    public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
    {
        return new HandlerResult()
        {
            ResponseMessage = "Введите имя пользователя в виде \"Логин:\"[вашеимя]. Пример: Логин:bot",
            Success = true,
        };
    }
}
