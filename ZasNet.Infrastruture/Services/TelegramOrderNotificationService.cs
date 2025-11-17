using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;

namespace ZasNet.Infrastruture.Services;

public class TelegramOrderNotificationService(
    ITelegramBotClient telegramBotClient,
    IOrderTelegramMessageBuilder messageBuilder,
    IOptions<TelegramSettings> telegramOptions,
    ILogger<TelegramOrderNotificationService> logger) : IOrderNotificationService
{
    private readonly ITelegramBotClient _telegramBotClient = telegramBotClient;
    private readonly IOrderTelegramMessageBuilder _messageBuilder = messageBuilder;
    private readonly TelegramSettings _settings = telegramOptions.Value;
    private readonly ILogger<TelegramOrderNotificationService> _logger = logger;

    public async Task NotifyOrderCreatedAsync(Order order, CancellationToken cancellationToken)
    {
        if (!_settings.IsEnabled)
        {
            return;
        }

        try
        {
            var chatId = BuildChatId(_settings.ChannelId);
            var text = _messageBuilder.BuildOrderMessage(order);
            var keyboard = _messageBuilder.BuildStatusKeyboard(order.Id, order.Status);

            await _telegramBotClient.SendMessage(chatId,
                text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram notification for order {OrderId}", order.Id);
        }
    }

    private static ChatId BuildChatId(string rawChatId)
    {
        if (long.TryParse(rawChatId, out var chatId))
        {
            return new ChatId(chatId);
        }

        return new ChatId(rawChatId);
    }
}

