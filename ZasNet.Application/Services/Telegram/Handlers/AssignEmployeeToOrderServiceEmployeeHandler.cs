using Microsoft.EntityFrameworkCore;
using System.Text;
using ZasNet.Application.Repository;
using ZasNet.Domain;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

/// <summary>
/// При подтверждении через меню свободные заявки
/// </summary>
/// <param name="repositoryManager"></param>
/// <param name="telegramBotAnswerService"></param>
/// <param name="freeOrdersCache"></param>
public class AssignEmployeeToOrderServiceEmployeeHandler(
	IRepositoryManager repositoryManager,
	ITelegramBotAnswerService telegramBotAnswerService,
	IFreeOrdersCache freeOrdersCache,
    IOrderServiceEmployeeApprovalService approvalService) : ITelegramMessageHandler
{
	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		var data = telegramUpdate?.CallbackQuery?.Data;
		if (!string.IsNullOrWhiteSpace(data)
			&& data.StartsWith("order:", StringComparison.OrdinalIgnoreCase)
			&& data.Contains(":orderservice:", StringComparison.OrdinalIgnoreCase))
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
		if (parts.Length >= 4 
			&& int.TryParse(parts[1], out var orderId)
			&& int.TryParse(parts[3], out var orderServiceId))
		{
			
			// Find employee by chat
			var employee = await repositoryManager.EmployeeRepository
				.FindByCondition(e => e.ChatId == chatId, true)
				.SingleOrDefaultAsync(cancellationToken);
            
			var order = await repositoryManager.OrderRepository
            .FindByCondition(c => c.Id == orderId, true)
            .SingleAsync(cancellationToken);
            if (order.Status != OrderStatus.Created)
            {
                await telegramBotAnswerService.SendMessageAsync(chatId, $"Данная заявка не в статусе создан", cancellationToken);
            }
            var lockedBy = order.IsLocked;

            if (lockedBy)
			{
				var lockedEmployee = await repositoryManager.EmployeeRepository.FindByCondition(c=>c.Id == order.LockedByUserId, false).Select(c=>c.Name).SingleOrDefaultAsync(cancellationToken);
				await telegramBotAnswerService.SendMessageAsync(chatId, $"Заявку редактирует {lockedEmployee}. Через некоторое время обновите список заявок и повторите операцию", cancellationToken);
				return new HandlerResult()
				{
					Success = true,
				};
			}


			if (employee != null)
			{
				// Load target order service with employees
				var orderService = await repositoryManager.OrderServiceRepository
					.FindByCondition(os => os.Id == orderServiceId && os.OrderId == orderId, true)
					.Include(os => os.OrderServiceEmployees)
					.SingleOrDefaultAsync(cancellationToken);

				if (orderService != null)
				{
					try
					{
						await repositoryManager.OrderRepository.LockItem(order.Id, employee.Id);
						// Skip if already assigned
						if (!orderService.OrderServiceEmployees.Any(ose => ose.EmployeeId == employee.Id))
						{
							var placeholder = orderService.OrderServiceEmployees
								.FirstOrDefault(ose => ose.EmployeeId == Constants.UnknowingEmployeeId);

							if(placeholder == null)
							{
								await telegramBotAnswerService.SendMessageAsync(chatId, $"Заявка уже принята другим сотрудником. Пожалуйста, обновите список", cancellationToken);
								return new HandlerResult { Success = false };
							}

							placeholder.EmployeeId = employee.Id;
							placeholder.IsApproved = true;
							await approvalService.UpdateOrderStatusAfterEmployeeApprovalAsync(orderId, placeholder.Id, cancellationToken);
							await repositoryManager.SaveAsync(cancellationToken);
						}
					}
					finally
					{
						await repositoryManager.OrderRepository.UnLockItem(orderId);
					}
				}
			}
			
			await telegramBotAnswerService.SendMessageAsync(chatId, $"Заявка успешно принята", cancellationToken);

		}

		// Invalidate cache to reflect changes and rebuild/send page 1
		freeOrdersCache.Invalidate(chatId);
		await SendFirstPageAsync(chatId, cancellationToken);

		return new HandlerResult { Success = false };
	}

	private async Task SendFirstPageAsync(long chatId, CancellationToken cancellationToken)
	{
		if (!freeOrdersCache.TryGet(chatId, out var pages))
		{
			// Load all free orders once
			var orders = await repositoryManager.OrderRepository
				.FindByCondition(o =>
					o.Status == OrderStatus.Created
					&& o.OrderServices.Any(os => os.OrderServiceEmployees.Any(c=>c.EmployeeId == Constants.UnknowingEmployeeId) && os.OrderServiceEmployees.Any(c => c.Employee.ChatId != chatId)),
					false)
				.Include(c=>c.OrderServices).ThenInclude(c=>c.Service)
				.Include(c=>c.OrderServices).ThenInclude(c=>c.OrderServiceEmployees).ThenInclude(c=>c.Employee)
				.Include(c=>c.OrderServices).ThenInclude(c=>c.OrderServiceCars).ThenInclude(c=>c.Car).ThenInclude(c=>c.CarModel)
				.OrderByDescending(o => o.CreatedDate)
				.ToListAsync(cancellationToken);

			if (orders.Count == 0)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Свободных заявок нет.", cancellationToken);
				return;
			}

			// Build cached pages (copy from FreeOrdersHandler to keep output consistent)
			pages = new List<CachedOrderPage>(orders.Count);
			foreach (var order in orders)
			{
				var serviesText = new StringBuilder();
				var buttons = new List<Button>();

				for (int i = 0; i < order.OrderServices.Count; i++)
				{
					serviesText.AppendLine();

					var service = order.OrderServices.ElementAt(i);

					// Заголовок услуги
					serviesText.AppendLine($"	🔧 Услуга {i + 1}: {service.Service.Name}");
					serviesText.AppendLine($"		💵 Цена: {service.Price:0.##} • 📦 Объем: {service.TotalVolume}");
					serviesText.AppendLine($"		🧮 Итого: {service.PriceTotal:0.##}");

					// Сотрудники
					var serviceEmployees = service.OrderServiceEmployees.Distinct().ToList();
					
					serviesText.AppendLine("	👷 Сотрудники:");
					for (int k = 0; k < serviceEmployees.Count; k++)
					{
						if (serviceEmployees[k].Employee.Id == Constants.UnknowingEmployeeId)
						{
							serviesText.AppendLine($"		🆓 Свободно ({k + 1})");
							buttons.Add(new Button { Text = $"Взять услугу {i + 1}", CallbackData = $"order:{service.OrderId}:orderservice:{service.Id}" });
						}
						else
						{
                            if (serviceEmployees[k].IsApproved)
                            {
                                serviesText.AppendLine($"		✅ {serviceEmployees[k].Employee.Name}");
                            }
                            else
                            {
                                serviesText.AppendLine($"		❓ {serviceEmployees[k].Employee.Name}");
                            }
						}
					}

					// Машины
					var orderServiceCars = service.OrderServiceCars.ToList();
					if (orderServiceCars.Count == 0)
					{
						serviesText.AppendLine("	🚗 Машины: пока не назначены");
					}
					else
					{
						serviesText.AppendLine("	🚗 Машины:");
						foreach (var car in orderServiceCars)
						{
                            if (car.IsApproved)
                            {
                                serviesText.AppendLine($"		✅ • {car.Car.CarModel.Name} ({car.Car.Number})");
                            }
                            else
                            {
                                serviesText.AppendLine($"		❓ • {car.Car.CarModel.Name} ({car.Car.Number})");
                            }
                        }
					}

					// Разделитель между услугами
					serviesText.AppendLine("━━━━━━━━━━━━━━━━━━━━");
				}


				var sb = new StringBuilder();
				sb.AppendLine("🆓 Свободная заявка");
				sb.AppendLine($"🧑 Клиент: {order.Client}");
				sb.AppendLine($"📍 Адрес: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
				sb.AppendLine($"🗓️ Дата: {order.Date:dd.MM.yyyy HH:mm}");
				sb.AppendLine();
				sb.AppendLine("🧾 Услуги:");
				sb.AppendLine(serviesText.ToString());
				sb.AppendLine($"💰 Общая сумма: {order.OrderPriceAmount:0.##}");
				sb.AppendLine($"💳 Оплата: {order.ClientType}");
				if (order.ClientType == ClientType.FizNal)
				{
					sb.AppendLine("‼️ Необходимо забрать оплату после выполнения!");
				}

				if (!string.IsNullOrWhiteSpace(order.Description))
				{
					sb.AppendLine();
					sb.AppendLine("📝 Комментарий:");
					sb.AppendLine(order.Description);
				}

				pages.Add(new CachedOrderPage
				{
					MessageText = sb.ToString(),
					Buttons = buttons
				});
			}

			// cache for 10 minutes
			freeOrdersCache.Set(chatId, pages, TimeSpan.FromMinutes(10));
		}

		var totalPages = Math.Max(1, pages.Count);
		var currentPage = 1;
		var pageIndex = 0;
		var page = pages[pageIndex];

		await telegramBotAnswerService.SendCachedOrderPageAsync(chatId, page.MessageText, page.Buttons, currentPage, totalPages, "free_orders", cancellationToken);
	}
}


