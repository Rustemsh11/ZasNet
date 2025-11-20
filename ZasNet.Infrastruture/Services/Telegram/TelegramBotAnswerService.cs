using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Services.Telegram;

public class TelegramBotAnswerService : ITelegramBotAnswerService
{
    private readonly ITelegramBotClient telegramBotClient;
    private readonly IOptions<TelegramSettings> telegramOptions;

    public TelegramBotAnswerService(
        ITelegramBotClient telegramBotClient,
        IOptions<TelegramSettings> telegramOptions)
    {
        this.telegramBotClient = telegramBotClient;
        this.telegramOptions = telegramOptions;
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

    public Task AnswerCallbackQueryAsync(string callbackQueryId, string? text = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
        //try
        //{
        //    var url = $"{_baseUrl}/answerCallbackQuery";
        //    var payload = new
        //    {
        //        callback_query_id = callbackQueryId,
        //        text = text,
        //        show_alert = false
        //    };

        //    var json = JsonSerializer.Serialize(payload);
        //    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        //    var response = await httpClient.PostAsync(url, content, cancellationToken);
        //    response.EnsureSuccessStatusCode();

        //    _logger.LogInformation("Callback query {CallbackQueryId} обработан", callbackQueryId);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Ошибка при обработке callback query {CallbackQueryId}", callbackQueryId);
        //    throw;
        //}
    }
}
