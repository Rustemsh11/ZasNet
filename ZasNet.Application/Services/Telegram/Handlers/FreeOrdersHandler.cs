using Microsoft.EntityFrameworkCore;
using System.Text;
using ZasNet.Application.Repository;
using ZasNet.Domain;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class FreeOrdersHandler(IRepositoryManager repositoryManager, ITelegramBotAnswerService telegramBotAnswerService, IFreeOrdersCache freeOrdersCache) : ITelegramMessageHandler
{
	private static readonly string CommandText = "Список свободных заявок";
	
	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		if (telegramUpdate?.Message?.Text == CommandText)
		{
			return true;
		}

		var data = telegramUpdate?.CallbackQuery?.Data;
		if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("free_orders:page:", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		return false;
	}

	public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
	{
		int pageSize = 1;
		int currentPage = 1;

		if (!string.IsNullOrWhiteSpace(telegramUpdate.CallbackQuery?.Data))
		{
			var parts = telegramUpdate.CallbackQuery.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			if (parts.Length >= 3 && int.TryParse(parts[^1], out var parsedPage) && parsedPage > 0)
			{
				currentPage = parsedPage;
			}
		}

		long chatId = telegramUpdate.Message?.From.ChatId ?? telegramUpdate.CallbackQuery!.From!.ChatId;

		// On explicit command press, reset cache to force fresh load
		if (telegramUpdate.Message?.Text == CommandText)
		{
			freeOrdersCache.Invalidate(chatId);
		}

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
				return new HandlerResult
				{
					Success = true,
					ResponseMessage = "Свободных заявок нет."
				};
			}

			// Build cached pages
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
					serviesText.AppendLine($"🔧 Услуга {i + 1}: {service.Service.Name}");
					serviesText.AppendLine($"   💵 Цена: {service.Price:0.##} • 📦 Объем: {service.TotalVolume}");
					serviesText.AppendLine($"   🧮 Итого: {service.PriceTotal:0.##}");

					// Сотрудники
					var serviceEmployees = service.OrderServiceEmployees.Distinct().ToList();
					if (serviceEmployees.Count == 0)
					{
						serviesText.AppendLine("👷 Сотрудники: пока не назначены");
						buttons.Add(new Button { Text = $"Взять услугу {i + 1}", CallbackData = $"order:{service.OrderId}:orderservice:{service.Id}" });
					}
					else
					{
						serviesText.AppendLine("👷 Сотрудники:");
						for (int k = 0; k < serviceEmployees.Count; k++)
						{
							if (serviceEmployees[k].Employee.Id == Constants.UnknowingEmployeeId)
							{
								serviesText.AppendLine($"   🆓 Свободно ({k + 1})");
								buttons.Add(new Button { Text = $"Взять услугу {i + 1}", CallbackData = $"order:{service.OrderId}:orderservice:{service.Id}" });
							}
							else
							{
								if (serviceEmployees[k].IsApproved)
								{
									serviesText.AppendLine($"   ✅ {serviceEmployees[k].Employee.Name}");
								}
								else
								{
                                    serviesText.AppendLine($"   ❓ {serviceEmployees[k].Employee.Name}");
                                }
							}
						}
					}

					// Машины
					var orderServiceCars = service.OrderServiceCars.ToList();
					if (orderServiceCars.Count == 0)
					{
						serviesText.AppendLine("🚗 Машины: пока не назначены");
					}
					else
					{
						serviesText.AppendLine("🚗 Машины:");
						foreach (var car in orderServiceCars)
						{
							if (car.IsApproved)
							{
								serviesText.AppendLine($"  ✅ • {car.Car.CarModel.Name} ({car.Car.Number})");
							}
							else
							{
                                serviesText.AppendLine($"  ❓ • {car.Car.CarModel.Name} ({car.Car.Number})");
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
		if (currentPage > totalPages) currentPage = totalPages;
		var pageIndex = Math.Max(0, currentPage - 1);
		var page = pages[pageIndex];

		await telegramBotAnswerService.SendCachedFreeOrderPageAsync(chatId, page.MessageText, page.Buttons, currentPage, totalPages, cancellationToken);

		return new HandlerResult { Success = false };

    }
}

