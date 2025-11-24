using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Telegram;

namespace ZasNet.Infrastruture.Services.Telegram;

public class TelegramBotAnswerService : ITelegramBotAnswerService
{
    private readonly ITelegramBotClient telegramBotClient;

    public TelegramBotAnswerService(
        ITelegramBotClient telegramBotClient,
        IOptions<TelegramSettings> telegramOptions)
    {
        this.telegramBotClient = telegramBotClient;
    }

    public async Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            await telegramBotClient.SendMessage(new ChatId(chatId), text, cancellationToken: cancellationToken);

        }
        catch (Exception ex)
        {
            // add liger
        }
    }

    public async Task SendCachedFreeOrderPageAsync(long chatId, string text, List<Button> buttons, int currentPage, int totalPages, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyboardRows = this.GetInlineButtons(buttons);

            // nav row
            if (totalPages > 1)
            {
                var navRow = new List<InlineKeyboardButton>(3);
                if (currentPage > 1)
                {
                    navRow.Add(InlineKeyboardButton.WithCallbackData("⟨ Назад", $"free_orders:page:{currentPage - 1}"));
                }

                navRow.Add(InlineKeyboardButton.WithCallbackData($"Стр {currentPage}/{totalPages}", "noop"));

                if (currentPage < totalPages)
                {
                    navRow.Add(InlineKeyboardButton.WithCallbackData("Вперед ⟩", $"free_orders:page:{currentPage + 1}"));
                }

                keyboardRows.Add(navRow.ToArray());
            }

            await telegramBotClient.SendMessage(new ChatId(chatId), text, replyMarkup: new InlineKeyboardMarkup(keyboardRows), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // add liger
        }
    }

    public Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    } //check to delete

    public async Task SendMessageWithMenuAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("Список моих открытых заявок") },
                new[] { new KeyboardButton("Список свободных заявок") },
                new[] { new KeyboardButton("Мои заявки за месяц") },
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false,
                Selective = false
            };

            await telegramBotClient.SendMessage(new ChatId(chatId), text, replyMarkup: keyboard, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // add liger
        }
    }

    public async Task SendMessageAsync(long chatId, string text, List<Button> buttons, CancellationToken cancellationToken = default)
    {
        try
        {
            await telegramBotClient.SendMessage(new ChatId(chatId), text, replyMarkup: new InlineKeyboardMarkup(this.GetInlineButtons(buttons)), cancellationToken: cancellationToken);

        }
        catch (Exception ex)
        {
            // add liger
        }
    }

    private List<InlineKeyboardButton[]> GetInlineButtons(List<Button> buttons)
    {
        var keyboardRows = new List<InlineKeyboardButton[]>();

        // service buttons (2 per row)
        for (int i = 0; i < buttons.Count; i += 2)
        {
            if (i + 1 < buttons.Count)
            {
                keyboardRows.Add(new[]
                {
                        InlineKeyboardButton.WithCallbackData(buttons[i].Text, buttons[i].CallbackData),
                        InlineKeyboardButton.WithCallbackData(buttons[i + 1].Text, buttons[i + 1].CallbackData)
                    });
            }
            else
            {
                keyboardRows.Add(new[]
                {
                        InlineKeyboardButton.WithCallbackData(buttons[i].Text, buttons[i].CallbackData)
                    });
            }
        }

        return keyboardRows;
    }
}
