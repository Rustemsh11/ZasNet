using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;
using static ZasNet.Domain.Entities.EmployeeEarinig;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class ProcessingOrderEditHandler(
	IRepositoryManager repositoryManager,
	ITelegramBotAnswerService telegramBotAnswerService,
	IFreeOrdersCache freeOrdersCache,
	IUserSessionCache userSessionCache,
	ITelegramFileService telegramFileService) : ITelegramMessageHandler
{
	private static readonly string CallbackRoot = "processing_orders";

	private static string? GetExtensionFromFileName(string? fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName)) return null;
		var idx = fileName.LastIndexOf('.');
		return idx >= 0 && idx < fileName.Length - 1 ? fileName[(idx + 1)..].ToLowerInvariant() : null;
	}

	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		var data = telegramUpdate?.CallbackQuery?.Data;
		if (!string.IsNullOrWhiteSpace(data))
		{
			if (data.StartsWith($"{CallbackRoot}:edit:order:", StringComparison.OrdinalIgnoreCase)) return true;
			if (data.StartsWith($"{CallbackRoot}:photos:start:", StringComparison.OrdinalIgnoreCase)) return true;
			if (data.StartsWith($"{CallbackRoot}:photos:done:", StringComparison.OrdinalIgnoreCase)) return true;
			if (data.StartsWith($"{CallbackRoot}:edit_service:", StringComparison.OrdinalIgnoreCase)) return true;
			if (data.StartsWith($"{CallbackRoot}:finish:", StringComparison.OrdinalIgnoreCase)) return true;
		}

		var chatId = telegramUpdate?.Message?.From?.ChatId ?? 0;
		if (chatId != 0 && userSessionCache.TryGet(chatId, out var session))
		{
			// Accept messages with text when awaiting input and photos when uploading
			if (telegramUpdate?.Message?.Text is not null && (session.Step == EditStep.AwaitingPrice || session.Step == EditStep.AwaitingVolume))
			{
				return true;
			}

			if (telegramUpdate?.Message?.Photo?.Any() == true && session.Step == EditStep.PhotoUploading)
			{
				return true;
			}

			if (telegramUpdate?.Message?.Document is not null && session.Step == EditStep.PhotoUploading)
			{
				return true;
			}
		}

		return false;
	}

	public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
	{
		if (!string.IsNullOrWhiteSpace(telegramUpdate.CallbackQuery?.Data))
		{
			var data = telegramUpdate.CallbackQuery!.Data;
			if (data.StartsWith($"{CallbackRoot}:edit:order:", StringComparison.OrdinalIgnoreCase))
			{
				return await HandleEditOrderCallbackAsync(telegramUpdate, cancellationToken);
			}

			if (data.StartsWith($"{CallbackRoot}:photos:start:", StringComparison.OrdinalIgnoreCase))
			{
				return await HandleStartPhotosAsync(telegramUpdate, cancellationToken);
			}

			if (data.StartsWith($"{CallbackRoot}:photos:done:", StringComparison.OrdinalIgnoreCase))
			{
				return await HandleFinishPhotosAsync(telegramUpdate, cancellationToken);
			}

			if (data.StartsWith($"{CallbackRoot}:edit_service:", StringComparison.OrdinalIgnoreCase))
			{
				return await HandleEditServiceActionAsync(telegramUpdate, cancellationToken);
			}

			if (data.StartsWith($"{CallbackRoot}:finish:", StringComparison.OrdinalIgnoreCase))
			{
				return await HandleFinishOrderAsync(telegramUpdate, cancellationToken);
			}
		}

		// Handle text input or photo upload in session
		if (telegramUpdate.Message?.From?.ChatId is long chatId && userSessionCache.TryGet(chatId, out var session))
		{
			if (telegramUpdate.Message.Photo?.Any() == true && session.Step == EditStep.PhotoUploading)
			{
				var largest = telegramUpdate.Message.Photo.OrderBy(p => p.FileSize ?? 0).Last();
				session.PhotoFileIds.Add(largest.FileId);
				session.LastUpdatedAt = DateTimeOffset.Now;
				userSessionCache.Set(session, TimeSpan.FromMinutes(30));

				await telegramBotAnswerService.SendMessageAsync(chatId, $"Фото добавлено. Всего: {session.PhotoFileIds.Count}. Отправьте ещё или нажмите «Готово».", cancellationToken);
				return new HandlerResult { Success = true };
			}

			if (telegramUpdate.Message.Document is not null && session.Step == EditStep.PhotoUploading)
			{
				var doc = telegramUpdate.Message.Document;
				session.PendingDocuments.Add(new PendingDocument
				{
					FileId = doc.FileId,
					FileName = doc.FileName,
					MimeType = doc.MimeType
				});
				session.LastUpdatedAt = DateTimeOffset.Now;
				userSessionCache.Set(session, TimeSpan.FromMinutes(30));

				await telegramBotAnswerService.SendMessageAsync(chatId, $"Документ добавлен. Всего: {session.PendingDocuments.Count}. Отправьте ещё или нажмите «Готово».", cancellationToken);
				return new HandlerResult { Success = true };
			}

			if (!string.IsNullOrWhiteSpace(telegramUpdate.Message?.Text))
			{
				return await HandleTextEntryAsync(telegramUpdate, session, cancellationToken);
			}
		}

		return new HandlerResult { Success = true };
	}

	private async Task<bool> CheckAndSetLock(int orderId, long chatId, CancellationToken cancellationToken)
	{
		var employeeId = await repositoryManager.OrderRepository.IsLockedBy(orderId);
		var currentEmployee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.ChatId == chatId, false).SingleAsync(cancellationToken);

        if (employeeId == null)
		{
            await repositoryManager.OrderRepository.LockItem(orderId, currentEmployee.Id);
			return true;
        }
		else
		{
			if(employeeId == currentEmployee.Id) 
			{
				return true;
			}

            var lockedEmployee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == employeeId.Value, false).Select(c => c.Name).SingleOrDefaultAsync(cancellationToken);
            await telegramBotAnswerService.SendMessageAsync(chatId, $"Заявку редактирует {lockedEmployee}. Через некоторое время обновите список заявок и повторите операцию", cancellationToken);
			return false;
		}
    }

	private async Task<HandlerResult> HandleEditOrderCallbackAsync(TelegramUpdate update, CancellationToken cancellationToken)
	{
		// processing_orders:edit:order:{orderId} OR processing_orders:edit:order:{orderId}:service:{serviceId}
		var parts = update.CallbackQuery!.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length >= 4 && int.TryParse(parts[3], out var orderId))
		{
            var chatId = update.CallbackQuery!.From!.ChatId;
            if (!(await CheckAndSetLock(orderId, chatId, cancellationToken)))
			{
                return new HandlerResult { Success = true };
            }

            if (parts.Length == 4)
			{
				// show service list
				return await ShowServicesListAsync(update, orderId, cancellationToken);
			}

			if (parts.Length >= 6 && parts[4] == "service" && int.TryParse(parts[5], out var serviceId))
			{
				return await ShowServiceEditMenuAsync(update, orderId, serviceId, cancellationToken);
			}
		}

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> ShowServicesListAsync(TelegramUpdate update, int orderId, CancellationToken cancellationToken)
	{
		var chatId = update.CallbackQuery!.From!.ChatId;
		var order = await repositoryManager.OrderRepository.FindByCondition(o => o.Id == orderId, false)
			.Include(o => o.OrderServices).ThenInclude(os => os.Service)
			.SingleOrDefaultAsync(cancellationToken);

		if (order == null)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка не найдена.", cancellationToken);
			return new HandlerResult { Success = true };
		}

		var buttons = new List<Button>();
		for (int i = 0; i < order.OrderServices.Count; i++)
		{
			var os = order.OrderServices.ElementAt(i);
			buttons.Add(new Button
			{
				Text = $"✏️ Услуга {i + 1}: {os.Service.Name}",
				CallbackData = $"{CallbackRoot}:edit:order:{orderId}:service:{os.Id}"
			});
		}

		await telegramBotAnswerService.SendMessageAsync(chatId, "Выберите услугу для изменения:", buttons, cancellationToken);

		var session = new EditOrderSession
		{
			ChatId = chatId,
			OrderId = orderId,
			Step = EditStep.SelectingService,
			LastUpdatedAt = DateTimeOffset.Now
		};
		userSessionCache.Set(session, TimeSpan.FromMinutes(30));

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> ShowServiceEditMenuAsync(TelegramUpdate update, int orderId, int orderServiceId, CancellationToken cancellationToken)
	{
		var chatId = update.CallbackQuery!.From!.ChatId;
		var os = await repositoryManager.OrderServiceRepository
			.FindByCondition(x => x.Id == orderServiceId && x.OrderId == orderId, false)
			.Include(x => x.Service)
			.SingleOrDefaultAsync(cancellationToken);

		if (os == null)
		{
			await telegramBotAnswerService.SendMessageAsync(chatId, "Услуга не найдена.", cancellationToken);
			return new HandlerResult { Success = true };
		}

		var sb = new StringBuilder();
		sb.AppendLine($"🔧 {os.Service.Name}");
		sb.AppendLine($"💵 Цена: {os.Price:0.##}");
		sb.AppendLine($"📦 Объем: {os.TotalVolume}");
		sb.AppendLine($"🧮 Итого: {os.PriceTotal:0.##}");

		var buttons = new List<Button>
		{
			new Button { Text = "💵 Изменить цену", CallbackData = $"{CallbackRoot}:edit_service:price:{orderId}:{orderServiceId}" },
			new Button { Text = "📦 Изменить объем", CallbackData = $"{CallbackRoot}:edit_service:volume:{orderId}:{orderServiceId}" },
			new Button { Text = "⬅️ Назад к услугам", CallbackData = $"{CallbackRoot}:edit:order:{orderId}" }
		};

		// store session
		userSessionCache.Set(new EditOrderSession
		{
			ChatId = chatId,
			OrderId = orderId,
			OrderServiceId = orderServiceId,
			Step = EditStep.SelectingService,
			LastUpdatedAt = DateTimeOffset.Now
		}, TimeSpan.FromMinutes(30));

		await telegramBotAnswerService.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> HandleEditServiceActionAsync(TelegramUpdate update, CancellationToken cancellationToken)
	{
		// processing_orders:edit_service:{price|volume}:{orderId}:{orderServiceId}
		var parts = update.CallbackQuery!.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 5 && int.TryParse(parts[3], out var orderId) && int.TryParse(parts[4], out var orderServiceId))
		{
            var chatId = update.CallbackQuery!.From!.ChatId;
            if (!(await CheckAndSetLock(orderId, chatId, cancellationToken)))
            {
                return new HandlerResult { Success = true };
            }
            var field = parts[2];
			if (field == "price")
			{
				userSessionCache.Set(new EditOrderSession
				{
					ChatId = chatId,
					OrderId = orderId,
					OrderServiceId = orderServiceId,
					Step = EditStep.AwaitingPrice,
					LastUpdatedAt = DateTimeOffset.Now
				}, TimeSpan.FromMinutes(30));

				await telegramBotAnswerService.SendMessageAsync(chatId, "Введите новую цену (например, 123.45):", cancellationToken);
				return new HandlerResult { Success = true };
			}
			if (field == "volume")
			{
				userSessionCache.Set(new EditOrderSession
				{
					ChatId = chatId,
					OrderId = orderId,
					OrderServiceId = orderServiceId,
					Step = EditStep.AwaitingVolume,
					LastUpdatedAt = DateTimeOffset.Now
				}, TimeSpan.FromMinutes(30));

				await telegramBotAnswerService.SendMessageAsync(chatId, "Введите новый объем (число, например, 2.5):", cancellationToken);
				return new HandlerResult { Success = true };
			}
		}

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> HandleTextEntryAsync(TelegramUpdate update, EditOrderSession session, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.From!.ChatId;
		var text = (update.Message!.Text ?? string.Empty).Trim().Replace(',', '.');

		if (session.Step == EditStep.AwaitingPrice)
		{
			if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var newPrice) || newPrice < 0)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Некорректная цена. Введите число, например 123.45", cancellationToken);
				return new HandlerResult { Success = true };
			}

			var os = await repositoryManager.OrderServiceRepository
				.FindByCondition(x => x.Id == session.OrderServiceId && x.OrderId == session.OrderId, true)
				.Include(os => os.OrderServiceEmployees)
				.Include(os => os.EmployeeEarinig)
				.Include(os => os.Service)
				.Include(os => os.Order)
					.ThenInclude(o => o.DispetcherEarning)
				.Include(os => os.Order)
					.ThenInclude(o => o.OrderServices)
				.Include(os => os.Order)
					.ThenInclude(o => o.CreatedEmployee)
				.SingleOrDefaultAsync(cancellationToken);
			if (os == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Услуга не найдена.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			os.Price = newPrice;
			os.PriceTotal = os.Price * (decimal)os.TotalVolume;

			// Обновление EmployeeEarning
			await UpdateEmployeeEarningForOrderService(os, cancellationToken);

			// Обновление DispetcherEarning
			await UpdateDispetcherEarningForOrder(os.Order, cancellationToken);

			await repositoryManager.SaveAsync(cancellationToken);

			freeOrdersCache.Invalidate(chatId);
			userSessionCache.Invalidate(chatId);

			await telegramBotAnswerService.SendMessageAsync(chatId, $"Цена обновлена: {newPrice:0.##}", cancellationToken);
			return new HandlerResult { Success = true };
		}

		if (session.Step == EditStep.AwaitingVolume)
		{
			if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var newVol) || newVol < 0)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Некорректный объем. Введите число, например 2.5", cancellationToken);
				return new HandlerResult { Success = true };
			}

			var os = await repositoryManager.OrderServiceRepository
				.FindByCondition(x => x.Id == session.OrderServiceId && x.OrderId == session.OrderId, true)
				.Include(os => os.OrderServiceEmployees)
				.Include(os => os.EmployeeEarinig)
				.Include(os => os.Service)
				.Include(os => os.Order)
					.ThenInclude(o => o.DispetcherEarning)
				.Include(os => os.Order)
					.ThenInclude(o => o.OrderServices)
				.Include(os => os.Order)
					.ThenInclude(o => o.CreatedEmployee)
				.SingleOrDefaultAsync(cancellationToken);
			if (os == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Услуга не найдена.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			os.TotalVolume = newVol;
			os.PriceTotal = os.Price * (decimal)os.TotalVolume;

			// Обновление EmployeeEarning
			await UpdateEmployeeEarningForOrderService(os, cancellationToken);

			// Обновление DispetcherEarning
			await UpdateDispetcherEarningForOrder(os.Order, cancellationToken);

			await repositoryManager.SaveAsync(cancellationToken);

			freeOrdersCache.Invalidate(chatId);
			userSessionCache.Invalidate(chatId);

			await telegramBotAnswerService.SendMessageAsync(chatId, $"Объем обновлен: {newVol}", cancellationToken);
			return new HandlerResult { Success = true };
		}

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> HandleStartPhotosAsync(TelegramUpdate update, CancellationToken cancellationToken)
	{
		// processing_orders:photos:start:{orderId}
		var parts = update.CallbackQuery!.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 4 && int.TryParse(parts[3], out var orderId))
		{
			var chatId = update.CallbackQuery!.From!.ChatId;
            if (!(await CheckAndSetLock(orderId, chatId, cancellationToken)))
            {
                return new HandlerResult { Success = true };
            }
            userSessionCache.Set(new EditOrderSession
			{
				ChatId = chatId,
				OrderId = orderId,
				Step = EditStep.PhotoUploading,
				LastUpdatedAt = DateTimeOffset.Now
			}, TimeSpan.FromMinutes(30));

			var buttons = new List<Button>
			{
				new Button { Text = "✅ Готово", CallbackData = $"{CallbackRoot}:photos:done:{orderId}" }
			};

			await telegramBotAnswerService.SendMessageAsync(chatId, "Отправьте фото отчета (можно несколько). Когда закончите, нажмите «Готово».", buttons, cancellationToken);
		}

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> HandleFinishPhotosAsync(TelegramUpdate update, CancellationToken cancellationToken)
	{
		// processing_orders:photos:done:{orderId}
		var parts = update.CallbackQuery!.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 4 && int.TryParse(parts[3], out var orderId))
		{
			var chatId = update.CallbackQuery!.From!.ChatId;

			if (!userSessionCache.TryGet(chatId, out var session) || session.Step != EditStep.PhotoUploading || session.OrderId != orderId)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Сессия загрузки фото не найдена или устарела.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			var employee = await repositoryManager.EmployeeRepository
				.FindByCondition(e => e.ChatId == chatId, false)
				.SingleOrDefaultAsync(cancellationToken);

			if (employee == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Пользователь не найден.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			if (session.PhotoFileIds.Count == 0 && session.PendingDocuments.Count == 0)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Файлы не получены. Пришлите хотя бы один файл", cancellationToken);
				return new HandlerResult { Success = true };
			}

			// Download and persist photos
			foreach (var fileId in session.PhotoFileIds)
			{
				var downloaded = await telegramFileService.DownloadAsync(fileId, cancellationToken);
				var extension = downloaded.Extension ?? "jpg";
				var contentType = downloaded.ContentType ?? "image/jpeg";

				var doc = new Document
				{
					Name = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}_{orderId}.{extension}",
					Extension = extension,
					Path = fileId, // keep original fileId for traceability
					Content = downloaded.Content,
					ContentType = contentType,
					SizeBytes = downloaded.SizeBytes,
					UploadedDate = DateTime.Now,
					UploadedUserId = employee.Id,
					OrderId = orderId,
					DocumentType = DocumentType.WorkReport
				};
				repositoryManager.DocumentRepository.Create(doc);
			}

			// Download and persist documents
			foreach (var pending in session.PendingDocuments)
			{
				var downloaded = await telegramFileService.DownloadAsync(pending.FileId, cancellationToken);
				var extension = downloaded.Extension ?? GetExtensionFromFileName(pending.FileName) ?? "bin";
				var contentType = pending.MimeType ?? downloaded.ContentType ?? "application/octet-stream";

				var safeName = string.IsNullOrWhiteSpace(pending.FileName)
					? $"doc_{DateTime.Now:yyyyMMdd_HHmmss}_{orderId}.{extension}"
					: pending.FileName!;

				var doc = new Document
				{
					Name = safeName,
					Extension = extension,
					Path = pending.FileId,
					Content = downloaded.Content,
					ContentType = contentType,
					SizeBytes = downloaded.SizeBytes,
					UploadedDate = DateTime.Now,
					UploadedUserId = employee.Id,
					OrderId = orderId,
					DocumentType = DocumentType.WorkReport
				};
				repositoryManager.DocumentRepository.Create(doc);
			}
			await repositoryManager.SaveAsync(cancellationToken);

			userSessionCache.Invalidate(chatId);

			var total = session.PhotoFileIds.Count + session.PendingDocuments.Count;
			await telegramBotAnswerService.SendMessageAsync(chatId, $"Файлы сохранены: {total} шт.", cancellationToken);
		}

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> HandleFinishOrderAsync(TelegramUpdate update, CancellationToken cancellationToken)
	{
		// processing_orders:finish:{orderId}
		var parts = update.CallbackQuery!.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 3 && int.TryParse(parts[2], out var orderId))
		{
			var chatId = update.CallbackQuery!.From!.ChatId;

			var employee = await repositoryManager.EmployeeRepository
				.FindByCondition(e => e.ChatId == chatId, false)
				.SingleOrDefaultAsync(cancellationToken);

			if (employee == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Пользователь не найден.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			var order = await repositoryManager.OrderRepository.FindByCondition(o => o.Id == orderId, true).SingleOrDefaultAsync(cancellationToken);
			if (order == null)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка не найдена.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			// Ensure at least one work report photo is attached
			var hasWorkReport = await repositoryManager.DocumentRepository
				.FindByCondition(d => d.OrderId == orderId && d.DocumentType == DocumentType.WorkReport, false)
				.AnyAsync(cancellationToken);

			if (!hasWorkReport)
			{
				// If user has an active photo-uploading session with photos, persist them now
				if (userSessionCache.TryGet(chatId, out var session) && session.Step == EditStep.PhotoUploading && (session.PhotoFileIds.Any() || session.PendingDocuments.Any()))
				{
					foreach (var fileId in session.PhotoFileIds)
					{
						var downloaded = await telegramFileService.DownloadAsync(fileId, cancellationToken);
						var extension = downloaded.Extension ?? "jpg";
						var contentType = downloaded.ContentType ?? "image/jpeg";

						var doc = new Document
						{
							Name = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}_{orderId}.{extension}",
							Extension = extension,
							Path = fileId,
							Content = downloaded.Content,
							ContentType = contentType,
							SizeBytes = downloaded.SizeBytes,
							UploadedDate = DateTime.Now,
							UploadedUserId = employee.Id,
							OrderId = orderId,
							DocumentType = DocumentType.WorkReport
						};
						repositoryManager.DocumentRepository.Create(doc);
					}

					foreach (var pending in session.PendingDocuments)
					{
						var downloaded = await telegramFileService.DownloadAsync(pending.FileId, cancellationToken);
						var extension = downloaded.Extension ?? GetExtensionFromFileName(pending.FileName) ?? "bin";
						var contentType = pending.MimeType ?? downloaded.ContentType ?? "application/octet-stream";

						var safeName = string.IsNullOrWhiteSpace(pending.FileName)
							? $"doc_{DateTime.Now:yyyyMMdd_HHmmss}_{orderId}.{extension}"
							: pending.FileName!;

						var doc = new Document
						{
							Name = safeName,
							Extension = extension,
							Path = pending.FileId,
							Content = downloaded.Content,
							ContentType = contentType,
							SizeBytes = downloaded.SizeBytes,
							UploadedDate = DateTime.Now,
							UploadedUserId = employee.Id,
							OrderId = orderId,
							DocumentType = DocumentType.WorkReport
						};
						repositoryManager.DocumentRepository.Create(doc);
					}
					await repositoryManager.SaveAsync(cancellationToken);
					userSessionCache.Invalidate(chatId);

					hasWorkReport = true;
				}
			}

			if (!hasWorkReport && (order.PaymentType == PaymentType.CashWithVat || order.PaymentType == PaymentType.CashWithoutVat))
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Нельзя завершить заявку без фотоотчёта. Отправьте фото и нажмите «Готово», либо прикрепите фото и повторите.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			// Set Finished and record who finished
			order.UpdateStatus(OrderStatus.Finished);
			order.FinishedEmployeeId = employee.Id;

			await repositoryManager.SaveAsync(cancellationToken);

			// Invalidate cached pages so this order disappears from processing list
			freeOrdersCache.Invalidate(chatId);
			await repositoryManager.OrderRepository.UnLockItem(orderId);
			await telegramBotAnswerService.SendMessageAsync(chatId, "Заявка переведена в статус «Finished».", cancellationToken);
		}

		return new HandlerResult { Success = true };
	}

	/// <summary>
	/// Обновляет EmployeeEarning для OrderService после изменения цены или объема
	/// </summary>
	private Task UpdateEmployeeEarningForOrderService(OrderService orderService, CancellationToken cancellationToken)
	{
		if (orderService.EmployeeEarinig != null)
		{
			// Удаляем старый EmployeeEarning
			repositoryManager.EmployeeEarningRepository.Delete(orderService.EmployeeEarinig);
		}

		// Создаем новый EmployeeEarning с обновленными данными
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

		orderService.EmployeeEarinig = EmployeeEarinig.CreateEmployeeEarning(createEmployeeEarningDto);

		return Task.CompletedTask;
	}

	/// <summary>
	/// Обновляет DispetcherEarning для Order после изменения цены или объема услуги
	/// </summary>
	private Task UpdateDispetcherEarningForOrder(Order order, CancellationToken cancellationToken)
	{
		if (order.DispetcherEarning != null && order.CreatedEmployee?.DispetcherProcent != null)
		{
			// Пересчитываем общую стоимость заявки
			var orderTotalPrice = order.OrderServices.Sum(os => os.PriceTotal);
			order.OrderPriceAmount = orderTotalPrice;

			// Обновляем заработок диспетчера
			order.DispetcherEarning.UpdateDispetcherEarning(
				order.CreatedEmployee.DispetcherProcent.Value,
				orderTotalPrice);
		}

		return Task.CompletedTask;
	}
}


