using ZasNet.Application.Repository;
using Microsoft.EntityFrameworkCore;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;
using System.Text;
using ZasNet.Domain;
using static ZasNet.Domain.Entities.EmployeeEarinig;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.Services.Telegram.Handlers;

/// <summary>
/// Обработчик для изменения водителей с множественным выбором
/// </summary>
public class ChangeOrderServiceEmployeesHandler(
	IRepositoryManager repositoryManager,
    ITelegramBotAnswerService telegramBotAnswerService) : ITelegramMessageHandler
{
	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		var data = telegramUpdate?.CallbackQuery?.Data;
		if (!string.IsNullOrWhiteSpace(data) && data.StartsWith("changemployees:", StringComparison.OrdinalIgnoreCase))
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
        // changemployees:{orderId} - начальная страница
        // changemployees:{orderId}:new:{orderServiceId} - начальная страница с новой услугой первой
        // changemployees:{orderId}:page:{serviceIndex1based} - навигация по услугам
        // changemployees:{orderId}:service:{orderServiceId}:toggle:employee:{employeeId}:index:{serviceIndex1based} - toggle водителя
        // changemployees:{orderId}:service:{orderServiceId}:confirm:index:{serviceIndex1based} - подтверждение выбора
        // changemployees:{orderId}:service:{orderServiceId}:confirm:index:{serviceIndex1based}:newservice - подтверждение выбора для новой услуги

        if (parts.Length >= 2 && int.TryParse(parts[1], out var orderId))
		{
			// Toggle employee selection
			if (parts.Contains("toggle", StringComparer.OrdinalIgnoreCase) && parts.Contains("employee", StringComparer.OrdinalIgnoreCase))
			{
				int orderServiceId = 0;
				int employeeId = 0;

				int serviceIdx = Array.FindIndex(parts, p => p.Equals("service", StringComparison.OrdinalIgnoreCase));
				int employeeIdx = Array.FindIndex(parts, p => p.Equals("employee", StringComparison.OrdinalIgnoreCase));
				int indexIdx = Array.FindIndex(parts, p => p.Equals("index", StringComparison.OrdinalIgnoreCase));

				if (serviceIdx >= 0 && serviceIdx + 1 < parts.Length) int.TryParse(parts[serviceIdx + 1], out orderServiceId);
				if (employeeIdx >= 0 && employeeIdx + 1 < parts.Length) int.TryParse(parts[employeeIdx + 1], out employeeId);

				var targetServiceIndex0 = 0;
				if (indexIdx >= 0 && indexIdx + 1 < parts.Length && int.TryParse(parts[indexIdx + 1], out var idxParsed) && idxParsed > 0)
				{
					targetServiceIndex0 = idxParsed - 1;
				}

				if (orderServiceId > 0 && employeeId > 0)
				{
					await ToggleEmployeeAsync(chatId, orderId, orderServiceId, employeeId, cancellationToken);
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

				// If this is a new service, redirect to car selection instead of next service
				if (isNewService)
				{
					var sb = new StringBuilder();
					sb.AppendLine("✅ Сотрудники выбраны!");
					sb.AppendLine();
					sb.AppendLine("Теперь выберите машины для этой услуги:");

					var buttons = new List<Button>
					{
						new Button
						{
							Text = "🚗 Выбрать машины",
							CallbackData = $"changeorderservicecar:{orderId}:new:{orderServiceId}"
						},
						new Button
						{
							Text = "⏭️ Пропустить",
							CallbackData = $"processing_orders:page:1"
						}
					};

					await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
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

	private async Task ToggleEmployeeAsync(long chatId, int orderId, int orderServiceId, int employeeId, CancellationToken cancellationToken)
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
            // Check if employee already assigned
            var orderService = await repositoryManager.OrderServiceRepository
				.FindByCondition(ose => ose.Id == orderServiceId, true)
				.Include(c=>c.Service)
				.Include(c=>c.Order)
				.Include(c=>c.OrderServiceEmployees)
				.Include(c=>c.EmployeeEarinig)
				.SingleOrDefaultAsync(cancellationToken);

			var orderServiceEmployee = orderService.OrderServiceEmployees.SingleOrDefault(c => c.EmployeeId == employeeId);


            if (orderServiceEmployee != null)
			{
				// Remove assignment (toggle off)
				repositoryManager.OrderEmployeeRepository.Delete(orderServiceEmployee);
				if(orderService.OrderServiceEmployees.Count == 1)
				{
                    AddNewEmployee(orderServiceId, Constants.UnknowingEmployeeId, orderService);
                }
				else
				{
					var createEmployeeEarningDto = new CreateEmployeeEarningDto()
					{
						PrecentForMultipleEmployeers = orderService.Service.PrecentForMultipleEmployeers,
						PrecentLaterOrderForEmployee = orderService.Service.PrecentLaterOrderForEmployee,
						PrecentLaterOrderForMultipleEmployeers = orderService.Service.PrecentLaterOrderForMultipleEmployeers,
						StandartPrecentForEmployee = orderService.Service.StandartPrecentForEmployee,
						OrderServiceEmployeesCount = orderService.OrderServiceEmployees.Count - 1,
						OrderStartDateTime = orderService.Order.DateStart,
						TotalPrice = orderService.PriceTotal,
					};
					orderService.EmployeeEarinig.Update(createEmployeeEarningDto);
				}

            }
			else
			{
				AddNewEmployee(orderServiceId, employeeId, orderService);

            }
            
			repositoryManager.OrderServiceRepository.Update(orderService);

			await repositoryManager.SaveAsync(cancellationToken);
		}
		finally
		{
			await repositoryManager.OrderRepository.UnLockItem(orderId);
		}
	}

	private void AddNewEmployee(int orderServiceId, int employeeId, OrderService orderService)
	{
        // Add assignment (toggle on)
        var newAssignment = new Domain.Entities.OrderServiceEmployee
        {
            OrderServiceId = orderServiceId,
            EmployeeId = employeeId,
            IsApproved = true,
        };
        repositoryManager.OrderEmployeeRepository.Create(newAssignment);

        var createEmployeeEarningDto = new CreateEmployeeEarningDto()
        {
            PrecentForMultipleEmployeers = orderService.Service.PrecentForMultipleEmployeers,
            PrecentLaterOrderForEmployee = orderService.Service.PrecentLaterOrderForEmployee,
            PrecentLaterOrderForMultipleEmployeers = orderService.Service.PrecentLaterOrderForMultipleEmployeers,
            StandartPrecentForEmployee = orderService.Service.StandartPrecentForEmployee,
            OrderServiceEmployeesCount = orderService.OrderServiceEmployees.Count,
            OrderStartDateTime = orderService.Order.DateStart,
            TotalPrice = orderService.PriceTotal,
        };

        orderService.EmployeeEarinig.Update(createEmployeeEarningDto);
    }

	private async Task SendServicePageAsync(long chatId, int orderId, int serviceIndex0, CancellationToken cancellationToken, int? newServiceId = null)
	{
		// Load order with services and employees
		var order = await repositoryManager.OrderRepository
			.FindByCondition(o => o.Id == orderId, false)
			.Include(o => o.OrderServices).ThenInclude(os => os.Service)
			.Include(o => o.OrderServices).ThenInclude(os => os.OrderServiceEmployees).ThenInclude(ose => ose.Employee)
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

		// Get currently selected employees
		var selectedEmployees = service.OrderServiceEmployees
			.Select(ose => ose.EmployeeId)
			.ToHashSet();

		// Load all employees to choose from
		var allEmployees = await repositoryManager.EmployeeRepository
			.FindByCondition(e => e.RoleId == Constants.DriversRole, false)
			.OrderBy(e => e.Name)
			.ToListAsync(cancellationToken);

		// Build message
		var sb = new StringBuilder();
		sb.AppendLine($"⚙️ Изменение водителей");
		sb.AppendLine($"🧑 Клиент: {order.Client}");
		sb.AppendLine($"📍 Адрес: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
		sb.AppendLine();
		sb.AppendLine($"🔧 Услуга: {service.Service.Name} ({serviceIndex0 + 1}/{services.Count})");
		sb.AppendLine($"💵 Цена: {service.Price:0.##} • 📦 Объем: {service.TotalVolume}");
		sb.AppendLine($"🧮 Итого: {service.PriceTotal:0.##}");
		sb.AppendLine();

		// Show selected employees
		if (selectedEmployees.Count > 0)
		{
			sb.AppendLine("👷 Выбранные водители:");
			foreach (var ose in service.OrderServiceEmployees.Where(ose => ose.EmployeeId != Constants.UnknowingEmployeeId))
			{
				sb.AppendLine($"  ✅ {ose.Employee.Name}");
			}
		}
		else
		{
			sb.AppendLine("👷 Водители: не выбраны");
		}
		sb.AppendLine();
		sb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
		sb.AppendLine("Выберите водителей:");

		var buttons = new List<Button>();

		// Employee selection buttons
		foreach (var emp in allEmployees)
		{
			var isSelected = selectedEmployees.Contains(emp.Id);
			var text = (isSelected ? "✅ " : "👤 ") + emp.Name;
			buttons.Add(new Button
			{
				Text = text,
				CallbackData = $"changemployees:{order.Id}:service:{service.Id}:toggle:employee:{emp.Id}:index:{serviceIndex0 + 1}"
			});
		}

		// Navigation and confirmation buttons
		// Determine if this is a new service (it's at index 0 and has newServiceId)
		bool isNewService = newServiceId.HasValue && service.Id == newServiceId.Value;

		if (services.Count > 1)
		{
			if (serviceIndex0 > 0)
			{
				buttons.Add(new Button
				{
					Text = "⟨ Предыдущая услуга",
					CallbackData = $"changemployees:{order.Id}:page:{serviceIndex0}"
				});
			}

			if (serviceIndex0 < services.Count - 1)
			{
				var confirmCallback = isNewService 
					? $"changemployees:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}:newservice"
					: $"changemployees:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}";
				
				buttons.Add(new Button
				{
					Text = isNewService ? "Продолжить ⟩" : "Следующая услуга ⟩",
					CallbackData = confirmCallback
				});
			}
			else
			{
				// Last service - just confirm
				var confirmCallback = isNewService 
					? $"changemployees:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}:newservice"
					: $"changemployees:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}";
				
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
				? $"changemployees:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}:newservice"
				: $"changemployees:{order.Id}:service:{service.Id}:confirm:index:{serviceIndex0 + 1}";
			
			buttons.Add(new Button
			{
				Text = "✅ Завершить",
				CallbackData = confirmCallback
			});
		}

		await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
	}
}

