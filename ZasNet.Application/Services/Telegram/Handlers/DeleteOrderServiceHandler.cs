using ZasNet.Application.Repository;
using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;
using System.Text;
using ZasNet.Domain;

namespace ZasNet.Application.Services.Telegram.Handlers;

/// <summary>
/// Обработчик для удаления услуг из заявки
/// </summary>
public class DeleteOrderServiceHandler(
	IRepositoryManager repositoryManager,
	ITelegramBotAnswerService telegramBotAnswerService,
	IFreeOrdersCache freeOrdersCache) : ITelegramMessageHandler
{
	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		var data = telegramUpdate?.CallbackQuery?.Data;
		if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("deleteorderservice:", StringComparison.OrdinalIgnoreCase))
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

		// Supported callbacks:
		// deleteorderservice:{orderId} - показать список услуг для удаления
		// deleteorderservice:{orderId}:confirm:{orderServiceId} - подтверждение удаления услуги

		if (parts.Length >= 2 && int.TryParse(parts[1], out var orderId))
		{
			// Confirm deletion
			if (parts.Contains("confirm", StringComparer.OrdinalIgnoreCase))
			{
				int orderServiceId = 0;
				int confirmIdx = Array.FindIndex(parts, p => p.Equals("confirm", StringComparison.OrdinalIgnoreCase));
				if (confirmIdx >= 0 && confirmIdx + 1 < parts.Length)
				{
					int.TryParse(parts[confirmIdx + 1], out orderServiceId);
				}

				if (orderServiceId > 0)
				{
					await DeleteOrderServiceAsync(chatId, orderId, orderServiceId, cancellationToken);
					return new HandlerResult { Success = true };
				}
			}

			// Show list of services to delete
			await ShowServicesListAsync(chatId, orderId, cancellationToken);
			return new HandlerResult { Success = true };
		}

		return new HandlerResult { Success = false };
	}

	private async Task DeleteOrderServiceAsync(long chatId, int orderId, int orderServiceId, CancellationToken cancellationToken)
	{
		var employee = await repositoryManager.EmployeeRepository
			.FindByCondition(e => e.ChatId == chatId, true)
			.SingleOrDefaultAsync(cancellationToken);

		var lockedBy = await repositoryManager.OrderRepository.IsLockedBy(orderId);
		if (lockedBy.HasValue && (employee == null || lockedBy.Value != employee.Id))
		{
			var lockedEmployee = await repositoryManager.EmployeeRepository
				.FindByCondition(c => c.Id == lockedBy.Value, false)
				.Select(c => c.Name)
				.SingleOrDefaultAsync(cancellationToken);
			await telegramBotAnswerService.SendMessageAsync(chatId, $"Заявку редактирует {lockedEmployee}. Через некоторое время попробуйте снова.", cancellationToken);
			return;
		}

		if (employee != null)
		{
			await repositoryManager.OrderRepository.LockItem(orderId, employee.Id);
		}

		try
		{
			var orderService = await repositoryManager.OrderServiceRepository
				.FindByCondition(os => os.Id == orderServiceId && os.OrderId == orderId, true)
				.Include(os => os.Service)
				.Include(os => os.Order)
				.ThenInclude(s=> s.DispetcherEarning)
				.SingleOrDefaultAsync(cancellationToken);

			if (orderService == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Услуга не найдена.", cancellationToken);
				return;
			}

			// Check if this is the last service
			var servicesCount = await repositoryManager.OrderServiceRepository
				.FindByCondition(os => os.OrderId == orderId, false)
				.CountAsync(cancellationToken);

			if (servicesCount <= 1)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "❌ Нельзя удалить последнюю услугу в заявке. Сначало добавьте новую, затем удлите ненужный.", cancellationToken);
				return;
			}

			var serviceName = orderService.Service.Name;
			var order = orderService.Order;

			// Delete the service (cascade will delete related entities)
			repositoryManager.OrderServiceRepository.Delete(orderService);

			// Recalculate order total price
			var remainingServices = await repositoryManager.OrderServiceRepository
				.FindByCondition(os => os.OrderId == orderId && os.Id != orderServiceId, false)
				.ToListAsync(cancellationToken);

			order.OrderPriceAmount = remainingServices.Sum(os => os.PriceTotal);
            var dispetcherProcent = (await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == order.CreatedEmployeeId, false).SingleOrDefaultAsync(cancellationToken))?.DispetcherProcent;
            order.DispetcherEarning.UpdateDispetcherEarning(dispetcherProcent.Value, order.OrderPriceAmount);
            repositoryManager.OrderRepository.Update(order);

			await repositoryManager.SaveAsync(cancellationToken);

			// Invalidate cache
			freeOrdersCache.Invalidate(chatId);

			await telegramBotAnswerService.SendMessageAsync(chatId, $"✅ Услуга '{serviceName}' успешно удалена. Обновите список заявок.", cancellationToken);
		}
		finally
		{
			await repositoryManager.OrderRepository.UnLockItem(orderId);
		}
	}

	private async Task ShowServicesListAsync(long chatId, int orderId, CancellationToken cancellationToken)
	{
		var order = await repositoryManager.OrderRepository
			.FindByCondition(o => o.Id == orderId, false)
			.Include(o => o.OrderServices).ThenInclude(os => os.Service)
			.SingleOrDefaultAsync(cancellationToken);

		if (order == null)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка не найдена.", cancellationToken);
			return;
		}

		if (order.OrderServices.Count == 0)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "В заявке нет услуг.", cancellationToken);
			return;
		}

		if (order.OrderServices.Count == 1)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "❌ В заявке только одна услуга. Нельзя удалить последнюю услугу.", cancellationToken);
			return;
		}

		var sb = new StringBuilder();
		sb.AppendLine("➖ Удаление услуги");
		sb.AppendLine($"🧑 Клиент: {order.Client}");
		sb.AppendLine($"📍 Адрес: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
		sb.AppendLine();
		sb.AppendLine("Выберите услугу для удаления:");

		var buttons = new List<Button>();

		foreach (var service in order.OrderServices.OrderBy(os => os.Id))
		{
			var text = $"🗑️ {service.Service.Name} ({service.PriceTotal:0.##})";
			buttons.Add(new Button
			{
				Text = text,
				CallbackData = $"deleteorderservice:{order.Id}:confirm:{service.Id}"
			});
		}

		// Cancel button
		buttons.Add(new Button
		{
			Text = "❌ Отмена",
			CallbackData = $"processing_orders:page:1"
		});

		await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
	}
}

