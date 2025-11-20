using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class SaveUserChatHandler(IRepositoryManager repositoryManager) : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        return telegramUpdate.Message.Text.StartsWith("Логин:");
    }

    public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
    {
        var userName = telegramUpdate.Message.Text.Substring("Логин:".Length).Trim();

        var employee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Name == userName, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Нет пользователя с логином {userName}");

        employee.SetChatId(telegramUpdate.Message.From.ChatId);

        await repositoryManager.SaveAsync(cancellationToken);

        return new HandlerResult()
        {
            ResponseMessage = "Успешно",
            Success = true,
        };
    }
}
