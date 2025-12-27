using Microsoft.EntityFrameworkCore;
using System.Text;
using ZasNet.Application.Repository;
using ZasNet.Domain;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Helpers;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class MyOpenOrdersHandler(IRepositoryManager repositoryManager,
    IFreeOrdersCache freeOrdersCache,
    ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
{
	private static readonly string CommandText = "Список моих открытых заявок";
    private static readonly string CallbackPrefix = "open_orders";

    public bool CanHandle(TelegramUpdate telegramUpdate)
	{
        if (telegramUpdate?.Message?.Text == CommandText)
        {
            return true;
        }

        var data = telegramUpdate?.CallbackQuery?.Data;
        if (!string.IsNullOrWhiteSpace(data) && data.StartsWith($"{CallbackPrefix}:page:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

	public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
	{
        long chatId = telegramUpdate.Message?.From.ChatId ?? telegramUpdate.CallbackQuery!.From!.ChatId;
		int currentPage = 1;

		if (!string.IsNullOrWhiteSpace(telegramUpdate.CallbackQuery?.Data))
		{
			var parts = telegramUpdate.CallbackQuery.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			if (parts.Length >= 3 && int.TryParse(parts[^1], out var parsedPage) && parsedPage > 0)
			{
				currentPage = parsedPage;
			}
		}

        // On explicit command press, reset cache to force fresh load
        if (telegramUpdate.Message?.Text == CommandText)
        {
            freeOrdersCache.Invalidate(chatId);
        }

        var employee = await repositoryManager.EmployeeRepository
			.FindByCondition(e => e.ChatId == chatId, false)
			.SingleOrDefaultAsync(cancellationToken);

		if (employee == null || employee.ChatId == null)
		{
            await telegramBotAnswerService.SendMessageAsync(chatId, "Ваш чат не привязан к пользователю. Отправьте \"Логин:ваш_логин\".");
            return new HandlerResult
			{
				Success = false,
			};
		}

        if (!freeOrdersCache.TryGet(employee.ChatId.Value, out var pages))
        {
			var orders = await repositoryManager.OrderRepository
				.FindByCondition(o =>
					(o.Status == OrderStatus.Created || o.Status == OrderStatus.ApprovedWithEmployers)
					&& o.OrderServices.Any(os => os.OrderServiceEmployees.Any(ose => ose.EmployeeId == employee.Id)),
					false)
				.Include(o => o.OrderServices).ThenInclude(os => os.Service)
				.Include(o => o.OrderServices).ThenInclude(os => os.OrderServiceEmployees).ThenInclude(ose => ose.Employee)
				.Include(o => o.OrderServices).ThenInclude(os => os.OrderServiceCars).ThenInclude(osc => osc.Car).ThenInclude(c => c.CarModel)
				.OrderByDescending(o => o.DateStart)
				.ToListAsync(cancellationToken);

			if (orders.Count == 0)
			{
				await telegramBotAnswerService.SendMessageAsync(employee.ChatId.Value, "У вас нет открытых заявок.");

				return new HandlerResult
				{
					Success = true,
				};
			}
            // Build cached pages (copy from FreeOrdersHandler to keep output consistent)
            pages = new List<CachedOrderPage>(orders.Count);
            foreach (var order in orders)
            {
                bool currentUserCanApproveCar = false;
                var serviesText = new StringBuilder();
                var buttons = new List<Button>();
                for (int i = 0; i < order.OrderServices.Count; i++)
                {
                    serviesText.AppendLine();

                    var service = order.OrderServices.ElementAt(i);

                    // Заголовок услуги
                    serviesText.AppendLine($"   🔧 Услуга {i + 1}: {service.Service.Name}");
                    serviesText.AppendLine($"       💵 Цена: {service.Price:0.##} • 📦 Объем: {service.TotalVolume}");
                    serviesText.AppendLine($"       🧮 Итого: {service.PriceTotal:0.##}");

                    // Сотрудники
                    var serviceEmployees = service.OrderServiceEmployees.Distinct().ToList();

                    serviesText.AppendLine("    👷 Сотрудники:");
                    for (int k = 0; k < serviceEmployees.Count; k++)
                    {
                        if (serviceEmployees[k].Employee.Id == Constants.UnknowingEmployeeId)
                        {
                            serviesText.AppendLine($"       🆓 Свободно ({k + 1})");
                            buttons.Add(new Button { Text = $"✅ услугу {i + 1}", CallbackData = $"order:{service.OrderId}:orderservice:{service.Id}" });
                        }
                        else
                        {
                            if (serviceEmployees[k].Employee.Id == employee.Id)
                            {
                                currentUserCanApproveCar = true;
                            }

                            if (serviceEmployees[k].Employee.Id == employee.Id && !serviceEmployees[k].IsApproved)
                            {
                                serviesText.AppendLine($"       ❓ {serviceEmployees[k].Employee.Name}");
                                buttons.Add(new Button { Text = $"✅ услугу {i + 1}", CallbackData = $"approveorderservice:{serviceEmployees[k].Id}" });
                            }
                            else
                            {
                                serviesText.AppendLine($"       ✅ {serviceEmployees[k].Employee.Name}");
                            }
                        }
                    }

                    // Машины
                    var orderServiceCars = service.OrderServiceCars.ToList();
                    if (orderServiceCars.Count == 0)
                    {
                        serviesText.AppendLine("    🚗 Машины: пока не назначены");
                    }
                    else
                    {
                        serviesText.AppendLine("    🚗 Машины:");
                        foreach (var car in orderServiceCars)
                        {
                            if (car.IsApproved)
                            {
                                serviesText.AppendLine($"       ✅ • {car.Car.CarModel.Name} ({car.Car.Number})");
                            }
                            else
                            {
                                serviesText.AppendLine($"       ❓ • {car.Car.CarModel.Name} ({car.Car.Number})");
                            }
                        }

                    }

                    // Разделитель между услугами
                    serviesText.AppendLine("━━━━━━━━━━━━━━━━━━━━");
                }

                if (currentUserCanApproveCar)
                {
                    buttons.Add(new Button { Text = $"✅ машины на выезд", CallbackData = $"approveorderservicecar:{order.Id}" });
                    buttons.Add(new Button { Text = $"🔄 изменить водителей", CallbackData = $"changemployees:{order.Id}" });
                    buttons.Add(new Button { Text = $"🔄 изменить машины", CallbackData = $"changeorderservicecar:{order.Id}" });
                }


                var sb = new StringBuilder();
                sb.AppendLine("🅼🆈 Моя заявка");
                sb.AppendLine($"🧑 Клиент: {order.Client}");
                sb.AppendLine($"📍 Адрес: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
                sb.AppendLine($"🗓️ Дата: {order.DateStart:dd.MM.yyyy HH:mm} - {order.DateEnd:dd.MM.yyyy HH:mm}");
                sb.AppendLine();
                sb.AppendLine("🧾 Услуги:");
                sb.AppendLine(serviesText.ToString());
                sb.AppendLine($"💰 Общая сумма: {order.OrderPriceAmount:0.##}");
                sb.AppendLine($"💳 Оплата: {EnumsToStringConverter.GetPaymentTypeDescription(order.PaymentType)}");
                if (order.PaymentType == PaymentType.Cash)
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
            freeOrdersCache.Set(employee.ChatId.Value, pages, TimeSpan.FromMinutes(10));
        }
    

        var totalPages = Math.Max(1, pages.Count);
		if (currentPage > totalPages) currentPage = totalPages;
		var pageIndex = Math.Max(0, currentPage - 1);
		var page = pages[pageIndex];
        await telegramBotAnswerService.SendCachedOrderPageAsync(employee.ChatId.Value, page.MessageText, page.Buttons, currentPage, totalPages, CallbackPrefix, cancellationToken);

		return new HandlerResult
		{
			Success = true,
		};
	}
}

