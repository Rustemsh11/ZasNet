using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class SaveUserChatHandler(IRepositoryManager repositoryManager, ITelegramBotAnswerService telegramBotAnswer) : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    { 
        if (telegramUpdate.Message?.Text == null)
        {
            return false; 
        }

        return telegramUpdate.Message!.Text!.StartsWith("Логин:");
    }

    public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
    {
        var userName = telegramUpdate.Message.Text.Substring("Логин:".Length).Trim();

        var employee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Login == userName, true).Include(c=>c.Role).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Нет пользователя с логином {userName}");

        employee.SetChatId(telegramUpdate.Message.From.ChatId);

        await repositoryManager.SaveAsync(cancellationToken);

        var generalLedger = await repositoryManager.RoleRepository.FindByCondition(c => c.Name == "Бухгалтер", false).SingleAsync();
        var isGeneralLedger = employee.Role.Id ==  generalLedger.Id;

        await telegramBotAnswer.SendMessageWithMenuAsync(employee.ChatId.Value, "Чат успешно сохранён. Выберите действие из меню ниже.", isGeneralLedger, cancellationToken);

        return new HandlerResult()
        {
            Success = true,
        };
    }
}
