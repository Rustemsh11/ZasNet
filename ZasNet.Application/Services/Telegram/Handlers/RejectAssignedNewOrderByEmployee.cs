using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using ZasNet.Application.Repository;
using ZasNet.Domain;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class RejectAssignedNewOrderByEmployee(
    IRepositoryManager repositoryManager,
    ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        var data = telegramUpdate?.CallbackQuery?.Data;
        if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("rejectorderservice:", StringComparison.OrdinalIgnoreCase))
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

            var serviceEmployee = await repositoryManager.OrderEmployeeRepository.FindByCondition(c => c.Id == orderServiceEmployeeId, true).Include(c=>c.OrderService).ThenInclude(c=>c.Service).SingleOrDefaultAsync(cancellationToken);

            if (serviceEmployee != null)
            {
                serviceEmployee.EmployeeId = Constants.UnknowingEmployeeId;
                serviceEmployee.IsApproved = false;
                await repositoryManager.SaveAsync(cancellationToken);
                await telegramBotAnswerService.SendMessageAsync(chatId, $"Услуга:[{serviceEmployee.OrderService.Service.Name}] успешно отменено");
            }
            else
            {
                await telegramBotAnswerService.SendMessageAsync(chatId, $"Не удалось отменить услугу:[{serviceEmployee.OrderService.Service.Name}]. Привязанного сотрудника к этой услуге не найдено.");
            }

        }

        return new HandlerResult()
        {
            Success = true,
        };
    }
}
