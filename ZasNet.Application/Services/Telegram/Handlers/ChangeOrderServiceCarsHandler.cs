using ZasNet.Application.Repository;
using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class ChangeOrderServiceCarsHandler(IRepositoryManager repositoryManager, ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
{
    public bool CanHandle(TelegramUpdate telegramUpdate)
    {
        var data = telegramUpdate?.CallbackQuery?.Data;
		if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("changeorderservicecar:", StringComparison.OrdinalIgnoreCase))
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
        // changeorderservicecar:{orderId}
        // changeorderservicecar:{orderId}:page:{serviceIndex1based}
        // changeorderservicecar:{orderId}:service:{orderServiceId}:servicecars:car:{carId}:index:{serviceIndex1based}
        if (parts.Length >= 2 && int.TryParse(parts[1], out var orderId))
		{
			// car selection branch
			if (parts.Contains("service", StringComparer.OrdinalIgnoreCase) && parts.Contains("car", StringComparer.OrdinalIgnoreCase))
			{
				int orderServiceId = 0;
				int servicecarsId = 0;
				int carId = 0;
				int index1 = Array.FindIndex(parts, p => p.Equals("service", StringComparison.OrdinalIgnoreCase));
				int index2 = Array.FindIndex(parts, p => p.Equals("servicecars", StringComparison.OrdinalIgnoreCase));
				int index3 = Array.FindIndex(parts, p => p.Equals("car", StringComparison.OrdinalIgnoreCase));
				int indexIdx = Array.FindIndex(parts, p => p.Equals("index", StringComparison.OrdinalIgnoreCase));

				if (index1 >= 0 && index1 + 1 < parts.Length) int.TryParse(parts[index1 + 1], out orderServiceId);
				if (index2 >= 0 && index2 + 1 < parts.Length) int.TryParse(parts[index2 + 1], out servicecarsId);
				if (index3 >= 0 && index3 + 1 < parts.Length) int.TryParse(parts[index3 + 1], out carId);

				var targetServiceIndex0 = 0;
				if (indexIdx >= 0 && indexIdx + 1 < parts.Length && int.TryParse(parts[indexIdx + 1], out var idxParsed) && idxParsed > 0)
				{
					targetServiceIndex0 = idxParsed - 1;
				}

				if (orderServiceId > 0 && carId > 0)
				{
					// lock and persist change
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
						return new HandlerResult { Success = true };
					}

					if (employee != null)
					{
						await repositoryManager.OrderRepository.LockItem(orderId, employee.Id);
					}

					try
					{
						var orderServiceCar = await repositoryManager.OrderCarRepository.FindByCondition(c => c.Id == servicecarsId, true).SingleOrDefaultAsync(cancellationToken);

						if (orderServiceCar != null)
						{
							// replace current selection with chosen car (single car per service for this flow)
							orderServiceCar.CarId = carId;
							repositoryManager.OrderCarRepository.Update(orderServiceCar);
							await repositoryManager.SaveAsync(cancellationToken);
						}


					}
					finally
					{
						await repositoryManager.OrderRepository.UnLockItem(orderId);
					}
					
					// re-render same page after update
					await SendServicePageAsync(chatId, orderId, targetServiceIndex0, cancellationToken);
					return new HandlerResult { Success = true };

				}
			}

			// page navigation / initial entry
			int currentIndex0 = 0;
			var pageIdx = Array.FindIndex(parts, p => p.Equals("page", StringComparison.OrdinalIgnoreCase));
			if (pageIdx >= 0 && pageIdx + 1 < parts.Length && int.TryParse(parts[pageIdx + 1], out var idx1based) && idx1based > 0)
			{
				currentIndex0 = idx1based - 1;
			}

			await SendServicePageAsync(chatId, orderId, currentIndex0, cancellationToken);
			return new HandlerResult { Success = true };
		}

		return new HandlerResult { Success = false };
    }

	private async Task SendServicePageAsync(long chatId, int orderId, int serviceIndex0, CancellationToken cancellationToken)
	{
		// Load order with services and cars
		var order = await repositoryManager.OrderRepository
			.FindByCondition(o => o.Id == orderId, false)
			.Include(o => o.OrderServices).ThenInclude(os => os.Service)
			.Include(o => o.OrderServices).ThenInclude(os => os.OrderServiceCars).ThenInclude(osc => osc.Car).ThenInclude(c => c.CarModel)
			.SingleOrDefaultAsync(cancellationToken);

		if (order == null)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка не найдена.", cancellationToken);
			return;
		}

		var services = order.OrderServices.OrderBy(os => os.Id).ToList();
		if (services.Count == 0)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "В заявке нет услуг.", cancellationToken);
			return;
		}

		if (serviceIndex0 < 0) serviceIndex0 = 0;
		if (serviceIndex0 >= services.Count) serviceIndex0 = services.Count - 1;

		var service = services[serviceIndex0];

		// Determine current selected car (take first if many)
		var currentCar = service.OrderServiceCars.FirstOrDefault();

		if(currentCar == null)
		{
			return;
		}

		// Load all cars to choose from
		var cars = await repositoryManager.CarRepository
			.FindAll(false)
			.Include(c => c.CarModel)
			.ToListAsync(cancellationToken);

		// Build message
		var header = $"⚙️ Изменение машины для заявки {order.Client}({order.AddressCity}, {order.AddressStreet}, {order.AddressNumber}\n" +
					 $"Услуга: {service.Service.Name} ({serviceIndex0 + 1}/{services.Count})\n" +
					 (currentCar != null
						 ? $"Текущая машина: {currentCar.Car.CarModel.Name} ({currentCar.Car.Number})"
						 : "Текущая машина: не назначена");

		// Build buttons: car options, then navigation, then approve
		var buttons = new List<Button>();
		foreach (var car in cars)
		{
			var isSelected = currentCar.Car != null && currentCar.CarId == car.Id;
			var text = (isSelected ? "✅ " : "🚗 ") + $"{car.CarModel.Name} ({car.Number})";
			buttons.Add(new Button
			{
				Text = text,
				CallbackData = $"changeorderservicecar:{order.Id}:service:{service.Id}:servicecars:{currentCar.Id}:car:{car.Id}:index:{serviceIndex0 + 1}"
			});
		}

		// Navigation
		if (services.Count > 1)
		{
			if (serviceIndex0 > 0)
			{
				buttons.Add(new Button
				{
					Text = "⟨ Предыдущая услуга",
					CallbackData = $"changeorderservicecar:{order.Id}:page:{serviceIndex0}"
				});
			}

			if (serviceIndex0 < services.Count - 1)
			{
				buttons.Add(new Button
				{
					Text = "Следующая услуга ⟩",
					CallbackData = $"changeorderservicecar:{order.Id}:page:{serviceIndex0 + 2}"
				});
			}
		}

		// Approve all cars for order button
		buttons.Add(new Button
		{
			Text = "✅ Подтвердить машины на выезд",
			CallbackData = $"approveorderservicecar:{order.Id}"
		});

		await telegramBotAnswerService.SendMessageAsync(chatId, header, buttons, cancellationToken);
	}
}
