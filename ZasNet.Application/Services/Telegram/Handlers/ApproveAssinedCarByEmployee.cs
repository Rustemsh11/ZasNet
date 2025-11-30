using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers
{
    public class ApproveAssinedCarByEmployee(IRepositoryManager repositoryManager, ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
    {
        public bool CanHandle(TelegramUpdate telegramUpdate)
        {
            var data = telegramUpdate?.CallbackQuery?.Data;
            if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("approveorderservicecar:", StringComparison.OrdinalIgnoreCase))
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
            && int.TryParse(parts[1], out var orderId))
            {
                var lockedBy = await repositoryManager.OrderRepository.IsLockedBy(orderId);

                try
                {
                    if (lockedBy.HasValue)
                    {
                        var lockedEmployee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == lockedBy.Value, false).Select(c => c.Name).SingleOrDefaultAsync(cancellationToken);
                        await telegramBotAnswerService.SendMessageAsync(chatId, $"Заявку редактирует {lockedEmployee}. Через некоторое время обновите список заявок и повторите операцию", cancellationToken);
                        return new HandlerResult()
                        {
                            Success = true,
                        };
                    }

                    var serviceCars = await repositoryManager.OrderCarRepository.FindByCondition(c => c.OrderService.OrderId == orderId, true)
                        .Include(c => c.OrderService).ThenInclude(c => c.Service)
                        .Include(c=>c.Car)
                        .ToListAsync(cancellationToken);
                    var employeeId = await repositoryManager.EmployeeRepository.FindByCondition(c => c.ChatId == chatId, false).Select(c=>c.Id).SingleOrDefaultAsync(cancellationToken);
                    if (employeeId != 0)
                    {
                        await repositoryManager.OrderRepository.LockItem(orderId, employeeId);
                    }
                
                    foreach (var serviceCar in serviceCars)
                    {
                        if (serviceCar != null)
                        {
                            serviceCar.IsApproved = true;
                        }
                    }

                    await repositoryManager.SaveAsync(cancellationToken);
                    await telegramBotAnswerService.SendMessageAsync(chatId, $"Выездные машины:[{string.Join(", ", serviceCars.Select(c=>c.Car.Number))}] успешно подтверждены");
                }
                finally
                {
                    await repositoryManager.OrderRepository.UnLockItem(orderId);
                }
            }

            return new HandlerResult()
            {
                Success = true,
            };
        }
    }
}
