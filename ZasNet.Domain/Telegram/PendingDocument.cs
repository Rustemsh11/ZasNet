namespace ZasNet.Domain.Telegram;

public class PendingDocument
{
	public string FileId { get; set; } = default!;
	public string? FileName { get; set; }
	public string? MimeType { get; set; }
}


