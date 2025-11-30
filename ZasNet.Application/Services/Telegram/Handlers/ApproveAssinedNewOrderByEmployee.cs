using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class ApproveAssinedNewOrderByEmployee(IOrderServiceEmployeeApprovalService approvalService, ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        var data = telegramUpdate?.CallbackQuery?.Data;
        if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("approveorderservice:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
    {
        long chatId = telegramUpdate.Message?.From.ChatId ?? telegramUpdate.CallbackQuery!.From!.ChatId;

        var data = telegramUpdate.CallbackQuery?.Data ?? string.Empty;
        var parts = data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2
            && int.TryParse(parts[1], out var orderServiceEmployeeId))
        {
            var result = await approvalService.ApproveAssignedAsync(orderServiceEmployeeId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                await telegramBotAnswerService.SendMessageAsync(chatId, result.Message, cancellationToken);
            }
        }

        return new HandlerResult()
        {
            Success = true,
        };
    }
}
