namespace ZasNet.Infrastruture.Services.Telegram;

public interface ITelegramBotAnswerService
{
    Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default);

    Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, CancellationToken cancellationToken = default);
}
