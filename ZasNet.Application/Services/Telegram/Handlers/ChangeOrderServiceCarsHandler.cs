using ZasNet.Application.Repository;
using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;
using System.Text;

namespace ZasNet.Application.Services.Telegram.Handlers;

/// <summary>
/// Обработчик для изменения машин с множественным выбором
/// </summary>
public class ChangeOrderServiceCarsHandler(
	IRepositoryManager repositoryManager,
	ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
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
		// changeorderservicecar:{orderId} - начальная страница
		// changeorderservicecar:{orderId}:new:{orderServiceId} - начальная страница с новой услугой первой
		// changeorderservicecar:{orderId}:page:{serviceIndex1based} - навигация по услугам
		// changeorderservicecar:{orderId}:service:{orderServiceId}:toggle:car:{carId}:index:{serviceIndex1based} - toggle машины
		// changeorderservicecar:{orderId}:service:{orderServiceId}:confirm:index:{serviceIndex1based} - подтверждение выбора
		// changeorderservicecar:{orderId}:service:{orderServiceId}:confirm:index:{serviceIndex1based}:newservice - подтверждение выбора для новой услуги

		if (parts.Length >= 2 && int.TryParse(parts[1], out var orderId))
		{
			// Toggle car selection
			if (parts.Contains("toggle", StringComparer.OrdinalIgnoreCase) && parts.Contains("car", StringComparer.OrdinalIgnoreCase))
			{
				int orderServiceId = 0;
				int carId = 0;

				int serviceIdx = Array.FindIndex(parts, p => p.Equals("service", StringComparison.OrdinalIgnoreCase));
				int carIdx = Array.FindIndex(parts, p => p.Equals("car", StringComparison.OrdinalIgnoreCase));
				int indexIdx = Array.FindIndex(parts, p => p.Equals("index", StringComparison.OrdinalIgnoreCase));

				if (serviceIdx >= 0 && serviceIdx + 1 < parts.Length) int.TryParse(parts[serviceIdx + 1], out orderServiceId);
				if (carIdx >= 0 && carIdx + 1 < parts.Length) int.TryParse(parts[carIdx + 1], out carId);

				var targetServiceIndex0 = 0;
				if (indexIdx >= 0 && indexIdx + 1 < parts.Length && int.TryParse(parts[indexIdx + 1], out var idxParsed) && idxParsed > 0)
				{
					targetServiceIndex0 = idxParsed - 1;
				}

				if (orderServiceId > 0 && carId > 0)
				{
					await ToggleCarAsync(chatId, orderId, orderServiceId, carId, cancellationToken);
					// Preserve newServiceId if we're at index 0 (the new service position)
					int? newServiceIdToPass = targetServiceIndex0 == 0 ? orderServiceId : null;
					await SendServicePageAsync(chatId, orderId, targetServiceIndex0, cancellationToken, newServiceIdToPass);
					return new HandlerResult { Success = true };
				}
			}

			// Confirm and move to next service
			if (parts.Contains("confirm", StringComparer.OrdinalIgnoreCase))
			{
				int orderServiceId = 0;
				int serviceIdx = Array.FindIndex(parts, p => p.Equals("service", StringComparison.OrdinalIgnoreCase));
				int indexIdx = Array.FindIndex(parts, p => p.Equals("index", StringComparison.OrdinalIgnoreCase));
				bool isNewService = parts.Contains("newservice", StringComparer.OrdinalIgnoreCase);

				if (serviceIdx >= 0 && serviceIdx + 1 < parts.Length) int.TryParse(parts[serviceIdx + 1], out orderServiceId);

				var targetServiceIndex0 = 0;
				if (indexIdx >= 0 && indexIdx + 1 < parts.Length && int.TryParse(parts[indexIdx + 1], out var idxParsed) && idxParsed > 0)
				{
					targetServiceIndex0 = idxParsed - 1;
				}

				// If this is a new service, just finish and return to orders list
				if (isNewService)
				{
					await telegramBotAnswerService.SendMessageAsync(chatId, "✅ Услуга успешно добавлена! Обновите список заявок.", cancellationToken);
					return new HandlerResult { Success = true };
				}

				// Load order to check if there are more services
				var order = await repositoryManager.OrderRepository
					.FindByCondition(o => o.Id == orderId, false)
					.Include(o => o.OrderServices)
					.SingleOrDefaultAsync(cancellationToken);

				if (order != null)
				{
					var services = order.OrderServices.OrderBy(os => os.Id).ToList();
					if (targetServiceIndex0 < services.Count - 1)
					{
						// Move to next service
						await SendServicePageAsync(chatId, orderId, targetServiceIndex0 + 1, cancellationToken);
					}
					else
					{
						// This was the last service
						await telegramBotAnswerService.SendMessageAsync(chatId, "✅ Все услуги обработаны. Обновите список заявок.", cancellationToken);
					}
				}

				return new HandlerResult { Success = true };
			}

			// Page navigation / initial entry
			int currentIndex0 = 0;
			int? newServiceId = null;

			// Check if this is a new service
			var newIdx = Array.FindIndex(parts, p => p.Equals("new", StringComparison.OrdinalIgnoreCase));
			if (newIdx >= 0 && newIdx + 1 < parts.Length && int.TryParse(parts[newIdx + 1], out var newSvcId))
			{
				newServiceId = newSvcId;
			}

			var pageIdx = Array.FindIndex(parts, p => p.Equals("page", StringComparison.OrdinalIgnoreCase));
			if (pageIdx >= 0 && pageIdx + 1 < parts.Length && int.TryParse(parts[pageIdx + 1], out var idx1based) && idx1based > 0)
			{
				currentIndex0 = idx1based - 1;
			}

			await SendServicePageAsync(chatId, orderId, currentIndex0, cancellationToken, newServiceId);
			return new HandlerResult { Success = true };
		}

		return new HandlerResult { Success = false };
	}

	private async Task ToggleCarAsync(long chatId, int orderId, int orderServiceId, int carId, CancellationToken cancellationToken)
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
			// Check if car already assigned
			var existingAssignment = await repositoryManager.OrderCarRepository
				.FindByCondition(osc => osc.OrderServiceId == orderServiceId && osc.CarId == carId, true)
				.SingleOrDefaultAsync(cancellationToken);

			if (existingAssignment != null)
			{
				// Remove assignment (toggle off)
				repositoryManager.OrderCarRepository.Delete(existingAssignment);
			}
			else
			{
				// Add assignment (toggle on)
				var newAssignment = new Domain.Entities.OrderServiceCar
				{
					OrderServiceId = orderServiceId,
					CarId = carId,
					IsApproved = true
				};
				repositoryManager.OrderCarRepository.Create(newAssignment);
			}

			await repositoryManager.SaveAsync(cancellationToken);
		}
		finally
		{
			await repositoryManager.OrderRepository.UnLockItem(orderId);
		}
	}

	private async Task SendServicePageAsync(long chatId, int orderId, int serviceIndex0, CancellationToken cancellationToken, int? newServiceId = null)
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

		// If newServiceId is provided, move it to the front
		if (newServiceId.HasValue)
		{
			var newService = services.FirstOrDefault(s => s.Id == newServiceId.Value);
			if (newService != null)
			{
				services.Remove(newService);
				services.Insert(0, newService);
				serviceIndex0 = 0; // Show the new service first
			}
		}
		if (services.Count == 0)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "В заявке нет услуг.", cancellationToken);
			return;
		}

		if (serviceIndex0 < 0) serviceIndex0 = 0;
		if (serviceIndex0 >= services.Count) serviceIndex0 = services.Count - 1;

		var service = services[serviceIndex0];

		// Get currently selected cars
		var selectedCars = service.OrderServiceCars.Select(osc => osc.CarId).ToHashSet();

		// Load all cars to choose from
		var allCars = await repositoryManager.CarRepository
			.FindAll(false)
			.Include(c => c.CarModel)
			.OrderBy(c => c.CarModel.Name)
			.ToListAsync(cancellationToken);

		// Build message
		var sb = new StringBuilder();
		sb.AppendLine($"⚙️ Изменение машин");
		sb.AppendLine($"🧑 Клиент: {order.Client}");
		sb.AppendLine($"📍 Адрес: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
		sb.AppendLine();
		sb.AppendLine($"🔧 Услуга: {service.Service.Name} ({serviceIndex0 + 1}/{services.Count})");
		sb.AppendLine($"💵 Цена: {service.Price:0.##} • 📦 Объем: {service.TotalVolume}");
		sb.AppendLine($"🧮 Итого: {service.PriceTotal:0.##}");
		sb.AppendLine();

		// Show selected cars
		if (selectedCars.Count > 0)
		{
			sb.AppendLine("🚗 Выбранные машины:");
			foreach (var osc in service.OrderServiceCars)
			{
				sb.AppendLine($"  ✅ {osc.Car.CarModel.Name} ({osc.Car.Number})");
			}
		}
		else
		{
			sb.AppendLine("🚗 Машины: не выбраны");
		}
		sb.AppendLine();
		sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
		sb.AppendLine("Выберите машины:");

		var buttons = new List<Button>();

		// Car selection buttons
		foreach (var car in allCars)
		{
			var isSelected = selectedCars.Contains(car.Id);
			var text = (isSelected ? "✅ " : "🚗 ") + $"{car.CarModel.Name} ({car.Number})";
			buttons.Add(new Button
			{
				Text = text,
				CallbackData = $"changeorderservicecar:{order.Id}:service:{service.Id}:toggle:car:{car.Id}:index:{serviceIndex0 + 1}"
			});
		}

		// Determine if this is a new service (it's at index 0 and has newServiceId)
		bool isNewService = newServiceId.HasValue && service.Id == newServiceId.Value;

		// Navigation and confirmation buttons
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
				var confirmCallback = isNewService 
					? $"changeorderservicecar:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}:newservice"
					: $"changeorderservicecar:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}";
				
				buttons.Add(new Button
				{
					Text = isNewService ? "Завершить ⟩" : "Следующая услуга ⟩",
					CallbackData = confirmCallback
				});
			}
			else
			{
				// Last service - just confirm
				var confirmCallback = isNewService 
					? $"changeorderservicecar:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}:newservice"
					: $"changeorderservicecar:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}";
				
				buttons.Add(new Button
				{
					Text = "✅ Завершить",
					CallbackData = confirmCallback
				});
			}
		}
		else
		{
			// Only one service
			var confirmCallback = isNewService 
				? $"changeorderservicecar:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}:newservice"
				: $"changeorderservicecar:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}";
			
			buttons.Add(new Button
			{
				Text = "✅ Завершить",
				CallbackData = confirmCallback
			});
		}

		await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
	}
}
