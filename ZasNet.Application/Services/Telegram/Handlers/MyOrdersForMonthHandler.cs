using Microsoft.EntityFrameworkCore;
using System.Text;
using ZasNet.Application.Repository;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class MyOrdersForMonthHandler(IRepositoryManager repositoryManager) : ITelegramMessageHandler
{
	private static readonly string CommandText = "Мои заявки за месяц";

	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		return telegramUpdate?.Message?.Text == CommandText;
	}

	public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
	{
		var employee = await repositoryManager.EmployeeRepository
			.FindByCondition(e => e.ChatId == telegramUpdate.Message.From.ChatId, false)
			.SingleOrDefaultAsync(cancellationToken);

		if (employee == null)
		{
			return new HandlerResult
			{
				Success = false,
				ResponseMessage = "Ваш чат не привязан к пользователю. Отправьте \"Логин:ваш_логин\"."
			};
		}

		var from = DateTime.UtcNow.AddDays(-30);

		var orders = await repositoryManager.OrderRepository
			.FindByCondition(o =>
				o.CreatedDate >= from
				&& o.OrderServices.Any(os => os.OrderServiceEmployees.Any(ose => ose.EmployeeId == employee.Id)),
				false)
			.OrderByDescending(o => o.CreatedDate)
			.Take(50)
			.ToListAsync(cancellationToken);

		if (orders.Count == 0)
		{
			return new HandlerResult
			{
				Success = true,
				ResponseMessage = "За последний месяц заявок не найдено."
			};
		}

		var sb = new StringBuilder();
		sb.AppendLine("Мои заявки за месяц:");
		foreach (var o in orders)
		{
			sb.AppendLine($"#{o.Id} | {o.Client}, {o.AddressCity} {o.AddressStreet} {o.AddressNumber} | {o.Status} | {o.CreatedDate:dd.MM.yyyy}");
		}

		return new HandlerResult
		{
			Success = true,
			ResponseMessage = sb.ToString()
		};
	}
}

