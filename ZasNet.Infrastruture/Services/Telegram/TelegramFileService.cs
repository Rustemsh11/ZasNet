using System.Net.Mime;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZasNet.Application.Services.Telegram;

namespace ZasNet.Infrastruture.Services.Telegram;

public class TelegramFileService : ITelegramFileService
{
	private readonly ITelegramBotClient telegramBotClient;

	public TelegramFileService(ITelegramBotClient telegramBotClient)
	{
		this.telegramBotClient = telegramBotClient;
	}

	public async Task<TelegramDownloadedFile> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
	{
		// Get file info
		var file = await telegramBotClient.GetFile(fileId, cancellationToken);

		// Download bytes
		await using var memory = new MemoryStream();
		await telegramBotClient.DownloadFile(file.FilePath!, memory, cancellationToken);
		var bytes = memory.ToArray();

		// Infer extension from file path if any
		string? extension = null;
		var filePath = file.FilePath;
		if (!string.IsNullOrWhiteSpace(filePath))
		{
			var dotIndex = filePath!.LastIndexOf('.');
			if (dotIndex >= 0 && dotIndex < filePath.Length - 1)
			{
				extension = filePath[(dotIndex + 1)..].ToLowerInvariant();
			}
		}

		// Best-effort content type guess
		string? contentType = extension switch
		{
			"jpg" or "jpeg" => "image/jpeg",
			"png" => "image/png",
			"gif" => "image/gif",
			"pdf" => "application/pdf",
			"doc" => "application/msword",
			"docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
			"xls" => "application/vnd.ms-excel",
			"xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
			_ => null
		};

		return new TelegramDownloadedFile
		{
			Content = bytes,
			FileName = file.FilePath,
			ContentType = contentType,
			Extension = extension
		};
	}
}
