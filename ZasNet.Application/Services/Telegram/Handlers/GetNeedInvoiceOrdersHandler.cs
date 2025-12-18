using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text;
using ZasNet.Application.Repository;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram.Handlers;

public class GetNeedInvoiceOrdersHandler(
	IRepositoryManager repositoryManager,
	ITelegramBotAnswerService telegramBotAnswerService,
	IFreeOrdersCache freeOrdersCache,
	ITelegramFileService telegramFileService) : ITelegramMessageHandler
{
	private static readonly string CommandText = "Заявки требующие счета";
	private static readonly string CallbackPrefix = "need_invoice_orders";

	private sealed class InvoiceUploadSession
	{
		public long ChatId { get; init; }
		public int OrderId { get; init; }
		public List<PendingDocument> PendingDocuments { get; } = new();
		public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.Now;
	}

	private static readonly ConcurrentDictionary<long, InvoiceUploadSession> invoiceSessions = new();

	public bool CanHandle(TelegramUpdate telegramUpdate)
	{
		if (telegramUpdate?.Message?.Text == CommandText)
		{
			return true;
		}

		var data = telegramUpdate?.CallbackQuery?.Data;
		if (!string.IsNullOrWhiteSpace(data))
		{
			if (data.StartsWith($"{CallbackPrefix}:page:", StringComparison.OrdinalIgnoreCase)) return true;
			if (data.StartsWith($"{CallbackPrefix}:invoice:start:", StringComparison.OrdinalIgnoreCase)) return true;
			if (data.StartsWith($"{CallbackPrefix}:invoice:done:", StringComparison.OrdinalIgnoreCase)) return true;
		}

		// Accept incoming documents while invoice session is active
		if (telegramUpdate?.Message?.From?.ChatId is long chatId && invoiceSessions.ContainsKey(chatId))
		{
			if (telegramUpdate?.Message?.Document is not null)
			{
				return true;
			}
		}

		return false;
	}

	public async Task<HandlerResult> HandleAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
	{
		// Callback-driven actions first
		if (!string.IsNullOrWhiteSpace(telegramUpdate.CallbackQuery?.Data))
		{
			var data = telegramUpdate.CallbackQuery!.Data;
			if (data.StartsWith($"{CallbackPrefix}:page:", StringComparison.OrdinalIgnoreCase))
			{
				return await SendPageAsync(telegramUpdate, cancellationToken);
			}

			if (data.StartsWith($"{CallbackPrefix}:invoice:start:", StringComparison.OrdinalIgnoreCase))
			{
				return await HandleStartInvoiceAsync(telegramUpdate, cancellationToken);
			}

			if (data.StartsWith($"{CallbackPrefix}:invoice:done:", StringComparison.OrdinalIgnoreCase))
			{
				return await HandleFinishInvoiceAsync(telegramUpdate, cancellationToken);
			}
		}

		// Incoming document during active invoice upload session
		if (telegramUpdate.Message?.Document is not null && telegramUpdate.Message.From?.ChatId is long docChatId && invoiceSessions.TryGetValue(docChatId, out var session))
		{
			var doc = telegramUpdate.Message.Document;
			session.PendingDocuments.Add(new PendingDocument
			{
				FileId = doc.FileId,
				FileName = doc.FileName,
				MimeType = doc.MimeType
			});
			session.LastUpdatedAt = DateTimeOffset.Now;

			await telegramBotAnswerService.SendMessageAsync(docChatId, $"Документ добавлен. Всего: {session.PendingDocuments.Count}. Отправьте ещё или нажмите «Готово».", cancellationToken);
			return new HandlerResult { Success = true };
		}

		// Default: initial command → list with pagination
		return await SendPageAsync(telegramUpdate, cancellationToken);
	}

	private async Task<HandlerResult> SendPageAsync(TelegramUpdate telegramUpdate, CancellationToken cancellationToken)
	{
		int currentPage = 1;
		int pageSize = 1;

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
			var orders = await repositoryManager.OrderRepository
				.GetOrders(c => true, Domain.Security.SecurityOperations.GeneralLedger, false)
				.OrderByDescending(o => o.CreatedDate)
				.ToListAsync(cancellationToken);

			if (orders.Count == 0)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Заявок, требующих выставления счета, нет.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			pages = new List<CachedOrderPage>(orders.Count);
			foreach (var order in orders)
			{
				var servicesSb = new StringBuilder();
				for (int i = 0; i < order.OrderServices.Count; i++)
				{
					var service = order.OrderServices.ElementAt(i);
					servicesSb.AppendLine($"	🔧 Услуга {i + 1}: {service.Service.Name}");
					servicesSb.AppendLine($"		💵 Цена: {service.Price:0.##}");
					servicesSb.AppendLine("━━━━━━━━━━━━━━━━━━━━");
				}

				var sb = new StringBuilder();
				sb.AppendLine("🧾 Заявка для выставления счета");
				sb.AppendLine($"🧑 Клиент: {order.Client}");
				sb.AppendLine($"🗓️ Период: {order.DateStart:dd.MM.yyyy HH:mm} — {order.DateEnd:dd.MM.yyyy HH:mm}");
				sb.AppendLine();
				sb.AppendLine("Услуги:");
				sb.AppendLine(servicesSb.ToString());

				var buttons = new List<Button>();

				// Attach ActOfCompletedWorks document link if present
				var actDoc = order.OrderDocuments.FirstOrDefault(d => d.DocumentType == DocumentType.ActOfCompletedWorks);
				if (actDoc is not null)
				{
					buttons.Add(new Button
					{
						Text = "📄 Акт выполненных работ",
						Url = $"https://h8742i-176-59-120-141.ru.tuna.am/api/v1/Document/Download?id={actDoc.Id}"
					});
				}

				// Add invoice button
				buttons.Add(new Button
				{
					Text = "➕ Добавить счет",
					CallbackData = $"{CallbackPrefix}:invoice:start:{order.Id}"
				});

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

		await telegramBotAnswerService.SendCachedOrderPageAsync(chatId, page.MessageText, page.Buttons, currentPage, totalPages, CallbackPrefix, cancellationToken);

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> HandleStartInvoiceAsync(TelegramUpdate update, CancellationToken cancellationToken)
	{
		// need_invoice_orders:invoice:start:{orderId}
		var parts = update.CallbackQuery!.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 4 && int.TryParse(parts[3], out var orderId))
		{
			var chatId = update.CallbackQuery!.From!.ChatId;

			var session = new InvoiceUploadSession
			{
				ChatId = chatId,
				OrderId = orderId
			};
			invoiceSessions.AddOrUpdate(chatId, session, (_, __) => session);

			var buttons = new List<Button>
			{
				new Button
				{
					Text = "Готово",
					CallbackData = $"{CallbackPrefix}:invoice:done:{orderId}"
				}
			};

			await telegramBotAnswerService.SendMessageAsync(chatId, "Отправьте файл(ы) счета. Когда закончите, нажмите «Готово».", buttons, cancellationToken);
		}

		return new HandlerResult { Success = true };
	}

	private async Task<HandlerResult> HandleFinishInvoiceAsync(TelegramUpdate update, CancellationToken cancellationToken)
	{
		// need_invoice_orders:invoice:done:{orderId}
		var parts = update.CallbackQuery!.Data.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (parts.Length == 4 && int.TryParse(parts[3], out var orderId))
		{
			var chatId = update.CallbackQuery!.From!.ChatId;

			if (!invoiceSessions.TryGetValue(chatId, out var session) || session.OrderId != orderId)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Сессия загрузки счетов не найдена или устарела.", cancellationToken);
				return new HandlerResult { Success = true };
			}

			if (session.PendingDocuments.Count == 0)
			{
				await telegramBotAnswerService.SendMessageAsync(chatId, "Файлы не получены. Пришлите хотя бы один файл или отмените.", cancellationToken);
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

			foreach (var pending in session.PendingDocuments)
			{
				var downloaded = await telegramFileService.DownloadAsync(pending.FileId, cancellationToken);
				var extension = downloaded.Extension ?? GetExtensionFromFileName(pending.FileName) ?? "bin";
				var contentType = pending.MimeType ?? downloaded.ContentType ?? "application/octet-stream";

				var safeName = string.IsNullOrWhiteSpace(pending.FileName)
					? $"invoice_{DateTime.Now:yyyyMMdd_HHmmss}_{orderId}.{extension}"
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
					DocumentType = DocumentType.Invoice
				};
				repositoryManager.DocumentRepository.Create(doc);
			}

			var order = await repositoryManager.OrderRepository.FindByCondition(c => c.Id == orderId, true).SingleAsync(cancellationToken);
			order.UpdateStatus(OrderStatus.AwaitingPayment);

			await repositoryManager.SaveAsync(cancellationToken);

			invoiceSessions.TryRemove(chatId, out _);

			var total = session.PendingDocuments.Count;
			await telegramBotAnswerService.SendMessageAsync(chatId, $"Счета сохранены. Всего файлов: {total}.", cancellationToken);

			// Invalidate cached pages so order view reflects changes
			freeOrdersCache.Invalidate(chatId);
		}

		return new HandlerResult { Success = true };
	}

	private static string? GetExtensionFromFileName(string? fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName)) return null;
		var idx = fileName.LastIndexOf('.');
		return idx >= 0 && idx < fileName.Length - 1 ? fileName[(idx + 1)..].ToLowerInvariant() : null;
	}
}
