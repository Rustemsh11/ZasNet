using ZasNet.Application.Repository;
using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;
using System.Text;
using ZasNet.Domain;
using ZasNet.Domain.Entities;
using static ZasNet.Domain.Entities.EmployeeEarinig;

namespace ZasNet.Application.Services.Telegram.Handlers;

/// <summary>
/// Обработчик для добавления новых услуг в заявку
/// </summary>
public class AddOrderServiceHandler(
	IRepositoryManager repositoryManager,
	ITelegramBotAnswerService telegramBotAnswerService,
	IFreeOrdersCache freeOrdersCache) : ITelegramMessageHandler
{
	// Временное хранилище для состояния добавления услуги
	private static readonly Dictionary<long, AddServiceState> _addServiceStates = new();

	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		var data = telegramUpdate?.CallbackQuery?.Data;
		var messageText = telegramUpdate?.Message?.Text;

		// Обрабатываем callback кнопки
		if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("addorderservice:", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		// Обрабатываем текстовые сообщения если пользователь в состоянии добавления услуги
		if (!string.IsNullOrWhiteSpace(messageText))
		{
			long chatId = telegramUpdate.Message.From.ChatId;
			if (_addServiceStates.ContainsKey(chatId))
			{
				var state = _addServiceStates[chatId];
				if (state.Stage == AddServiceStage.WaitingForPrice || state.Stage == AddServiceStage.WaitingForVolume)
				{
					return true;
				}
			}
		}

		return false;
	}

	public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
	{
		long chatId = telegramUpdate.Message?.From.ChatId ?? telegramUpdate.CallbackQuery!.From!.ChatId;

		// Handle text input for price/volume
		if (!string.IsNullOrWhiteSpace(telegramUpdate.Message?.Text) && _addServiceStates.ContainsKey(chatId))
		{
			var state = _addServiceStates[chatId];
			
			if (state.Stage == AddServiceStage.WaitingForPrice)
			{
				await HandlePriceInputAsync(chatId, telegramUpdate.Message.Text, state, cancellationToken);
				return new HandlerResult { Success = true };
			}
			
			if (state.Stage == AddServiceStage.WaitingForVolume)
			{
				await HandleVolumeInputAsync(chatId, telegramUpdate.Message.Text, state, cancellationToken);
				return new HandlerResult { Success = true };
			}
		}

		var data = telegramUpdate.CallbackQuery?.Data ?? string.Empty;
		var parts = data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		// Supported callbacks:
		// addorderservice:{orderId} - показать список услуг
		// addorderservice:{orderId}:selectservice:{serviceId} - выбрать услугу
		// addorderservice:{orderId}:skipprice - пропустить ввод цены
		// addorderservice:{orderId}:skipvolume - пропустить ввод объема
		// addorderservice:{orderId}:selectemployees:{orderServiceId} - выбрать сотрудников (после создания)
		// addorderservice:{orderId}:selectcars:{orderServiceId} - выбрать машины (после выбора сотрудников)
		// addorderservice:{orderId}:complete:{orderServiceId} - завершить добавление

		if (parts.Length >= 2 && int.TryParse(parts[1], out var orderId))
		{
			// Select service
			if (parts.Contains("selectservice", StringComparer.OrdinalIgnoreCase))
			{
				int serviceId = 0;
				int serviceIdx = Array.FindIndex(parts, p => p.Equals("selectservice", StringComparison.OrdinalIgnoreCase));
				if (serviceIdx >= 0 && serviceIdx + 1 < parts.Length)
				{
					int.TryParse(parts[serviceIdx + 1], out serviceId);
				}

				if (serviceId > 0)
				{
					await HandleServiceSelectionAsync(chatId, orderId, serviceId, cancellationToken);
					return new HandlerResult { Success = true };
				}
			}

			// Skip price
			if (parts.Contains("skipprice", StringComparer.OrdinalIgnoreCase))
			{
				await HandleSkipPriceAsync(chatId, cancellationToken);
				return new HandlerResult { Success = true };
			}

			// Skip volume
			if (parts.Contains("skipvolume", StringComparer.OrdinalIgnoreCase))
			{
				await HandleSkipVolumeAsync(chatId, cancellationToken);
				return new HandlerResult { Success = true };
			}

			// Complete addition after all selections
			if (parts.Contains("complete", StringComparer.OrdinalIgnoreCase))
			{
				int orderServiceId = 0;
				int completeIdx = Array.FindIndex(parts, p => p.Equals("complete", StringComparison.OrdinalIgnoreCase));
				if (completeIdx >= 0 && completeIdx + 1 < parts.Length)
				{
					int.TryParse(parts[completeIdx + 1], out orderServiceId);
				}

				if (orderServiceId > 0)
				{
					await CompleteAdditionAsync(chatId, orderId, orderServiceId, cancellationToken);
					return new HandlerResult { Success = true };
				}
			}

			// Show services list (initial call)
			await ShowServicesListAsync(chatId, orderId, cancellationToken);
			return new HandlerResult { Success = true };
		}

		return new HandlerResult { Success = false };
	}

	private async Task ShowServicesListAsync(long chatId, int orderId, CancellationToken cancellationToken)
	{
		var order = await repositoryManager.OrderRepository
			.FindByCondition(o => o.Id == orderId, false)
			.SingleOrDefaultAsync(cancellationToken);

		if (order == null)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка не найдена.", cancellationToken);
			return;
		}

		var services = await repositoryManager.ServiceRepository
			.FindAll(false)
			.Include(s => s.Measure)
			.OrderBy(s => s.Name)
			.ToListAsync(cancellationToken);

		if (services.Count == 0)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Нет доступных услуг.", cancellationToken);
			return;
		}

		var sb = new StringBuilder();
		sb.AppendLine("➕ Добавление услуги");
		sb.AppendLine($"🧑 Клиент: {order.Client}");
		sb.AppendLine($"📍 Адрес: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
		sb.AppendLine();
		sb.AppendLine("Выберите услугу:");

		var buttons = new List<Button>();

		foreach (var service in services)
		{
			var text = $"{service.Name} ({service.Price:0.##}/{service.Measure.Name})";
			buttons.Add(new Button
			{
				Text = text,
				CallbackData = $"addorderservice:{orderId}:selectservice:{service.Id}"
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

	private async Task HandleServiceSelectionAsync(long chatId, int orderId, int serviceId, CancellationToken cancellationToken)
	{
		var service = await repositoryManager.ServiceRepository
			.FindByCondition(s => s.Id == serviceId, false)
			.Include(s => s.Measure)
			.SingleOrDefaultAsync(cancellationToken);

		if (service == null)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Услуга не найдена.", cancellationToken);
			return;
		}

		var order = await repositoryManager.OrderRepository
			.FindByCondition(o => o.Id == orderId, false)
			.SingleOrDefaultAsync(cancellationToken);

		if (order == null)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка не найдена.", cancellationToken);
			return;
		}

		// Initialize state
		_addServiceStates[chatId] = new AddServiceState
		{
			OrderId = orderId,
			ServiceId = serviceId,
			MinPrice = service.Price,
			MinVolume = service.MinVolume,
			Price = service.Price,
			Volume = service.MinVolume,
			Stage = AddServiceStage.WaitingForPrice,
			Service = service,
			Order = order
		};

		var sb = new StringBuilder();
		sb.AppendLine($"Услуга: {service.Name}");
		sb.AppendLine($"Минимальная цена: {service.Price:0.##} за {service.Measure.Name}");
		sb.AppendLine();
		sb.AppendLine("Введите новую цену или нажмите 'Пропустить' для использования минимальной:");

		var buttons = new List<Button>
		{
			new Button
			{
				Text = "⏭️ Пропустить",
				CallbackData = $"addorderservice:{orderId}:skipprice"
			}
		};

		await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
	}

	private async Task HandlePriceInputAsync(long chatId, string input, AddServiceState state, CancellationToken cancellationToken)
	{
		if (decimal.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price) && price > 0)
		{
			state.Price = price;
			await AskForVolumeAsync(chatId, state, cancellationToken);
		}
		else
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "❌ Неверный формат цены. Введите положительное число:", cancellationToken);
		}
	}

	private async Task HandleSkipPriceAsync(long chatId, CancellationToken cancellationToken)
	{
		if (_addServiceStates.TryGetValue(chatId, out var state))
		{
			state.Price = state.MinPrice;
			await AskForVolumeAsync(chatId, state, cancellationToken);
		}
	}

	private async Task AskForVolumeAsync(long chatId, AddServiceState state, CancellationToken cancellationToken)
	{
		state.Stage = AddServiceStage.WaitingForVolume;

		var sb = new StringBuilder();
		sb.AppendLine($"Услуга: {state.Service.Name}");
		sb.AppendLine($"Цена: {state.Price:0.##} за {state.Service.Measure.Name}");
		sb.AppendLine($"Минимальный объем: {state.MinVolume}");
		sb.AppendLine();
		sb.AppendLine("Введите объем или нажмите 'Пропустить' для использования минимального:");

		var buttons = new List<Button>
		{
			new Button
			{
				Text = "⏭️ Пропустить",
				CallbackData = $"addorderservice:{state.OrderId}:skipvolume"
			}
		};

		await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
	}

	private async Task HandleVolumeInputAsync(long chatId, string input, AddServiceState state, CancellationToken cancellationToken)
	{
		if (double.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var volume) && volume > 0)
		{
			state.Volume = volume;
			await CreateOrderServiceAsync(chatId, state, cancellationToken);
		}
		else
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "❌ Неверный формат объема. Введите положительное число:", cancellationToken);
		}
	}

	private async Task HandleSkipVolumeAsync(long chatId, CancellationToken cancellationToken)
	{
		if (_addServiceStates.TryGetValue(chatId, out var state))
		{
			state.Volume = state.MinVolume;
			await CreateOrderServiceAsync(chatId, state, cancellationToken);
		}
	}

	private async Task CreateOrderServiceAsync(long chatId, AddServiceState state, CancellationToken cancellationToken)
	{
		var employee = await repositoryManager.EmployeeRepository
			.FindByCondition(e => e.ChatId == chatId, true)
			.SingleOrDefaultAsync(cancellationToken);

		var lockedBy = await repositoryManager.OrderRepository.IsLockedBy(state.OrderId);
		if (lockedBy.HasValue && (employee == null || lockedBy.Value != employee.Id))
		{
			var lockedEmployee = await repositoryManager.EmployeeRepository
				.FindByCondition(c => c.Id == lockedBy.Value, false)
				.Select(c => c.Name)
				.SingleOrDefaultAsync(cancellationToken);
			await telegramBotAnswerService.SendMessageAsync(chatId, $"Заявку редактирует {lockedEmployee}. Через некоторое время попробуйте снова.", cancellationToken);
			_addServiceStates.Remove(chatId);
			return;
		}

		if (employee != null)
		{
			await repositoryManager.OrderRepository.LockItem(state.OrderId, employee.Id);
		}

		try
		{
			// Reload order with tracking
			var order = await repositoryManager.OrderRepository
				.FindByCondition(o => o.Id == state.OrderId, true)
				.Include(o => o.OrderServices)
				.Include(o=>o.DispetcherEarning)
				.SingleOrDefaultAsync(cancellationToken);

			if (order == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка не найдена.", cancellationToken);
				_addServiceStates.Remove(chatId);
				return;
			}

			// Reload service
			var service = await repositoryManager.ServiceRepository
				.FindByCondition(s => s.Id == state.ServiceId, false)
				.SingleOrDefaultAsync(cancellationToken);

			if (service == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Услуга не найдена.", cancellationToken);
				_addServiceStates.Remove(chatId);
				return;
			}

			// Create new OrderService
			var priceTotal = state.Price * (decimal)state.Volume;
			var orderService = new OrderService
			{
				OrderId = state.OrderId,
				ServiceId = state.ServiceId,
				Price = state.Price,
				TotalVolume = state.Volume,
				PriceTotal = priceTotal,
				OrderServiceEmployees = new List<OrderServiceEmployee>
				{
					new OrderServiceEmployee
					{
						EmployeeId = Constants.UnknowingEmployeeId,
						IsApproved = false
					}
				},
				OrderServiceCars = new List<OrderServiceCar>()
			};

			// Create EmployeeEarning
			var createEmployeeEarningDto = new CreateEmployeeEarningDto
			{
				PrecentForMultipleEmployeers = service.PrecentForMultipleEmployeers,
				PrecentLaterOrderForEmployee = service.PrecentLaterOrderForEmployee,
				PrecentLaterOrderForMultipleEmployeers = service.PrecentLaterOrderForMultipleEmployeers,
				StandartPrecentForEmployee = service.StandartPrecentForEmployee,
				OrderServiceEmployeesCount = 1,
				OrderStartDateTime = order.DateStart,
				OrderEndDateTime = order.DateEnd,
				TotalPrice = priceTotal
			};

			orderService.EmployeeEarinig = EmployeeEarinig.CreateEmployeeEarning(createEmployeeEarningDto);

			repositoryManager.OrderServiceRepository.Create(orderService);

			// Update order total price
			order.OrderPriceAmount += priceTotal;
            var dispetcherProcent = (await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == order.CreatedEmployeeId, false).SingleOrDefaultAsync(cancellationToken))?.DispetcherProcent;
			order.DispetcherEarning.UpdateDispetcherEarning(dispetcherProcent.Value, order.OrderPriceAmount);
			repositoryManager.OrderRepository.Update(order);

			await repositoryManager.SaveAsync(cancellationToken);

			// Get the created OrderService ID
			var createdOrderServiceId = orderService.Id;

			// Invalidate cache
			freeOrdersCache.Invalidate(chatId);

			// Clean up state
			_addServiceStates.Remove(chatId);

			// Redirect to employee selection using existing handler
			var sb = new StringBuilder();
			sb.AppendLine($"✅ Услуга '{service.Name}' добавлена!");
			sb.AppendLine($"💰 Цена: {state.Price:0.##} × {state.Volume} = {priceTotal:0.##}");
			sb.AppendLine();
			sb.AppendLine("Теперь выберите сотрудников для этой услуги:");

			var buttons = new List<Button>
			{
				new Button
				{
					Text = "👷 Выбрать сотрудников",
					CallbackData = $"changemployees:{state.OrderId}:new:{createdOrderServiceId}"
				},
				new Button
				{
					Text = "⏭️ Пропустить",
					CallbackData = $"processing_orders:page:1"
				}
			};

			await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
		}
		finally
		{
			await repositoryManager.OrderRepository.UnLockItem(state.OrderId);
		}
	}

	private async Task CompleteAdditionAsync(long chatId, int orderId, int orderServiceId, CancellationToken cancellationToken)
	{
		// Clean up state
		_addServiceStates.Remove(chatId);

		// Invalidate cache
		freeOrdersCache.Invalidate(chatId);

		await telegramBotAnswerService.SendMessageAsync(chatId, "✅ Услуга успешно добавлена. Обновите список заявок.", cancellationToken);
	}

	private class AddServiceState
	{
		public int OrderId { get; set; }
		public int ServiceId { get; set; }
		public decimal MinPrice { get; set; }
		public double MinVolume { get; set; }
		public decimal Price { get; set; }
		public double Volume { get; set; }
		public AddServiceStage Stage { get; set; }
		public Service Service { get; set; } = null!;
		public Order Order { get; set; } = null!;
	}

	private enum AddServiceStage
	{
		WaitingForPrice,
		WaitingForVolume,
		SelectingEmployees,
		SelectingCars,
		Complete
	}
}

