using ZasNet.Domain.Entities;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram;

public interface ITelegramBotAnswerService
{
    Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default);

    Task SendMessageAsync(long chatId, string text, List<Button> buttons, CancellationToken cancellationToken = default);

	Task SendCachedOrderPageAsync(long chatId, string text, List<Button> buttons, int currentPage, int totalPages, string callbackPrefix, CancellationToken cancellationToken = default);

    Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, CancellationToken cancellationToken = default);

    Task SendMessageWithMenuAsync(long chatId, string text, CancellationToken cancellationToken = default);
}
