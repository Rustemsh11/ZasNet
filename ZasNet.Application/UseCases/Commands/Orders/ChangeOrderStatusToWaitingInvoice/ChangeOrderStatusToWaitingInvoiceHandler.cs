using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services.Telegram;

namespace ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatusToWaitingInvoice;

public class ChangeOrderStatusToWaitingInvoiceHandler(IRepositoryManager repositoryManager, ITelegramBotAnswerService telegramBotAnswerService) : IRequestHandler<ChangeOrderStatusToWaitingInvoiceCommand>
{
    public async Task Handle(ChangeOrderStatusToWaitingInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await repositoryManager.OrderRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException("Заявка не найдена");

        if(order.Status != Domain.Enums.OrderStatus.Finished)
        {
            return;
        }

        order.UpdateStatus(Domain.Enums.OrderStatus.CreatingInvoice);
        order.NeedInvoiceUrgently = request.isNeedInvoiceArgently;

        await repositoryManager.SaveAsync(cancellationToken);

        if (request.isNeedInvoiceArgently)
        {
            var generalLedger = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Role.Name == "Бухгалтер", false).SingleOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Бухгалтер не найден");

            if(generalLedger.ChatId != null)
            {
                await telegramBotAnswerService.SendMessageAsync(generalLedger.ChatId.Value, "Появилась заявка, требующая срочного счета. Посмотрите список 'Срочные заявки'", cancellationToken);
            }
        }
    }
}
