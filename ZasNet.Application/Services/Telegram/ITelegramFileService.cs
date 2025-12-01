using System.Threading;

namespace ZasNet.Application.Services.Telegram;

public sealed class TelegramDownloadedFile
{
	public required byte[] Content { get; init; }
	public string? FileName { get; init; }
	public string? ContentType { get; init; }
	public string? Extension { get; init; }
	public long SizeBytes => Content.LongLength;
}

public interface ITelegramFileService
{
	Task<TelegramDownloadedFile> DownloadAsync(string fileId, CancellationToken cancellationToken = default);
}

