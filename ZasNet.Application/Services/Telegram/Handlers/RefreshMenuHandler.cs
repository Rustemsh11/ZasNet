using ZasNet.Application.Repository;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class RefreshMenuHandler(IRepositoryManager repositoryManager, ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        return telegramUpdate?.Message?.Text == "/меню";
    }

    public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
    {
        var chatId = telegramUpdate.Message?.From?.ChatId;
        if (chatId == null)
        {
            return new HandlerResult() { Success = true };
        }

        var employee = repositoryManager.EmployeeRepository.FindByCondition(c => c.ChatId == chatId, false).SingleOrDefault();
        if (employee == null)
        {
            await telegramBotAnswerService.SendMessageAsync(chatId.Value, "Не получилось вас найти в базе, отправьте сообщение /start", cancellationToken);
            return new HandlerResult() { Success = true };
        }

        await telegramBotAnswerService.SendMessageWithMenuAsync(chatId.Value, "Выберите действие из меню ниже.", cancellationToken);
        
        return new HandlerResult() { Success = true };
    }
}
