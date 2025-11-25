using Telegram.Bot.Types;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram;

public class TelegramMessageProcessor
{
    private IEnumerable<ITelegramMessageHandler> handlers;
    private ITelegramBotAnswerService telegramBotAnswerService;

    public TelegramMessageProcessor(IEnumerable<ITelegramMessageHandler> handlers, ITelegramBotAnswerService telegramBotAnswerService)
    {
        this.handlers = handlers;
        this.telegramBotAnswerService = telegramBotAnswerService;
    }

    public async Task<HandlerResult> ProcessAsync(TelegramUpdate update, CancellationToken cancellationToken = default)
    {
        var chatId = update.Message?.From?.ChatId ?? update.CallbackQuery?.From?.ChatId ?? 0;
        
        try
        {
            var handler = handlers.FirstOrDefault(h => h.CanHandle(update));
            if (handler == null)
            {
                
                await telegramBotAnswerService.SendMessageAsync(chatId, "Неизвестный тип сообщения");
                return new HandlerResult
                {
                    Success = false,
                };
            }

            return await handler.HandleAsync(update, cancellationToken);
        }
        catch (Exception ex)
        {
            await telegramBotAnswerService.SendMessageAsync(chatId, $"Произошла ошибка при обработке сообщения:{ex.Message}");
            return new HandlerResult
            {
                Success = false,
            };
        }
    }
}
