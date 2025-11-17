using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatus;

public class ChangeOrderStausFromTgHandler(
    IRepositoryManager repositoryManager,
    ITelegramValidate telegramValidate,
    IOrderTelegramMessageBuilder messageBuilder,
    ILogger<ChangeOrderStausFromTgHandler> logger,
    ITelegramBotClient? telegramBotClient = null) : IRequestHandler<ChangeOrderStausFromTgCommand, ChangeOrderStausFromTgResult>
{
    private readonly IRepositoryManager _repositoryManager = repositoryManager;
    private readonly ITelegramValidate _telegramValidate = telegramValidate;
    private readonly IOrderTelegramMessageBuilder _messageBuilder = messageBuilder;
    private readonly ILogger<ChangeOrderStausFromTgHandler> _logger = logger;
    private readonly ITelegramBotClient? _telegramBotClient = telegramBotClient;

    public async Task<ChangeOrderStausFromTgResult> Handle(ChangeOrderStausFromTgCommand request, CancellationToken cancellationToken)
    {
        if (!_telegramValidate.IsEnabled())
        {
            return ChangeOrderStausFromTgResult.Success();
        }

        if (!_telegramValidate.IsSecretValid(request.Secret))
        {
            _logger.LogWarning("Rejected Telegram webhook call with invalid secret");
            return ChangeOrderStausFromTgResult.Unauthorized();
        }

        if (_telegramBotClient is null)
        {
            _logger.LogWarning("Telegram webhook invoked but bot client is not configured");
            return ChangeOrderStausFromTgResult.ServiceUnavailable();
        }

        if (request.Update.Type == UpdateType.CallbackQuery && request.Update.CallbackQuery is not null)
        {
            await HandleCallbackQuery(request.Update.CallbackQuery, cancellationToken);
        }
        else if (request.Update.Type == UpdateType.Message && request.Update.Message is not null)
        {
            await HandleMessage(request.Update.Message, cancellationToken);
        }

        return ChangeOrderStausFromTgResult.Success();
    }

    private async Task HandleMessage(Message message, CancellationToken cancellationToken)
    {
        // Basic text message handling (e.g. commands like /start)
        if (message.From is null || !_telegramValidate.IsUserAllowed(message.From.Id))
        {
            if (message.Chat is not null)
            {
                await _telegramBotClient!.SendMessage(message.Chat.Id,
                    "You do not have access to this bot.",
                    cancellationToken: cancellationToken);
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return;
        }

        var text = message.Text.Trim();

        if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
        {
            await _telegramBotClient!.SendMessage(message.Chat.Id,
                "Welcome to ZasNet order bot.\nUse the buttons in order messages to change order status.",
                cancellationToken: cancellationToken);
            return;
        }

        // Fallback for unsupported messages
        await _telegramBotClient!.SendMessage(message.Chat.Id,
            "Unsupported command. Please use the buttons attached to order messages to change status.",
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (!_telegramValidate.IsUserAllowed(callbackQuery.From.Id))
        {
            await _telegramBotClient!.AnswerCallbackQuery(callbackQuery.Id,
                "You do not have access to change order status.",
                showAlert: true,
                cancellationToken: cancellationToken);
            return;
        }

        if (!TryParseCallback(callbackQuery.Data, out var orderId, out var status))
        {
            await _telegramBotClient!.AnswerCallbackQuery(callbackQuery.Id,
                "Cannot parse selected status.",
                showAlert: true,
                cancellationToken: cancellationToken);
            return;
        }

        var order = await _repositoryManager.OrderRepository
            .FindByCondition(o => o.Id == orderId, true)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            await _telegramBotClient!.AnswerCallbackQuery(callbackQuery.Id,
                $"Order #{orderId} not found.",
                showAlert: true,
                cancellationToken: cancellationToken);
            return;
        }

        order.UpdateStatus(status);
        _repositoryManager.OrderRepository.Update(order);
        await _repositoryManager.SaveAsync(cancellationToken);

        await UpdateTelegramMessage(callbackQuery, order, cancellationToken);

        await _telegramBotClient!.AnswerCallbackQuery(callbackQuery.Id,
            $"Order #{order.Id} status updated to {status}",
            cancellationToken: cancellationToken);
    }

    private async Task UpdateTelegramMessage(CallbackQuery callbackQuery, Order order, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message is null)
        {
            return;
        }

        var updatedText = _messageBuilder.BuildOrderMessage(order);
        var markup = _messageBuilder.BuildStatusKeyboard(order.Id, order.Status);

        await _telegramBotClient!.EditMessageText(callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            updatedText,
            replyMarkup: markup,
            cancellationToken: cancellationToken);
    }

    private static bool TryParseCallback(string? data, out int orderId, out OrderStatus status)
    {
        orderId = default;
        status = default;

        if (string.IsNullOrWhiteSpace(data))
        {
            return false;
        }

        var parts = data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out orderId))
        {
            return false;
        }

        if (!int.TryParse(parts[3], out var statusValue))
        {
            return false;
        }

        status = (OrderStatus)statusValue;
        return true;
    }
}
