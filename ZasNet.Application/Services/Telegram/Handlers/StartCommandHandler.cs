using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class StartCommandHandler(ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        return telegramUpdate?.Message?.Text == "/start";
    }

    public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
    {
        await telegramBotAnswerService.SendMessageAsync(telegramUpdate.Message.From.ChatId, "Введите имя пользователя в виде \"Логин:\"[вашеимя]. Пример: Логин:bot");
        return new HandlerResult()
        {
            Success = true,
        };
    }
}
