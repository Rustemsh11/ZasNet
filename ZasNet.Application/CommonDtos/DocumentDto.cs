using ZasNet.Domain.Enums;

namespace ZasNet.Application.CommonDtos;

public class DocumentDto
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Extension { get; set; }
	public string? ContentType { get; set; }
	public long? SizeBytes { get; set; }
	public DateTime UploadedDate { get; set; }
	public DocumentType DocumentType { get; set; }

	// Relative URLs to be used by the web client
	public string ViewUrl { get; set; }
	public string DownloadUrl { get; set; }

	public bool IsImage => (ContentType ?? string.Empty).StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}

