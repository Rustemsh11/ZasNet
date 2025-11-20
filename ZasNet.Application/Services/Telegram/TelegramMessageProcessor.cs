using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram;

public class TelegramMessageProcessor
{
    private IEnumerable<ITelegramMessageHandler> handlers;

    public TelegramMessageProcessor(IEnumerable<ITelegramMessageHandler> handlers)
    {
        this.handlers = handlers;
    }

    public async Task<HandlerResult> ProcessAsync(TelegramUpdate update, CancellationToken cancellationToken = default)
    {
        try
        {
            var handler = handlers.FirstOrDefault(h => h.CanHandle(update));

            if (handler == null)
            {
                return new HandlerResult
                {
                    Success = false,
                    ResponseMessage = "Неизвестный тип сообщения"
                };
            }

            return await handler.HandleAsync(update, cancellationToken);
        }
        catch (Exception ex)
        {
            return new HandlerResult
            {
                Success = false,
                ResponseMessage = $"Произошла ошибка при обработке сообщения:{ex.Message}",
            };
        }
    }
}
