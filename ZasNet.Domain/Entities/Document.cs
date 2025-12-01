using ZasNet.Domain.Enums;

namespace ZasNet.Domain.Entities;

public class Document : LockedItemBase
{
    public string Name { get; set; }

    public string Extension { get; set; }

    public string? Path { get; set; }

    // Binary content of the file (stored in SQL Server as VARBINARY(MAX))
    public byte[]? Content { get; set; }

    // MIME type, e.g. "image/jpeg", "application/pdf"
    public string? ContentType { get; set; }

    // Size in bytes of Content
    public long? SizeBytes { get; set; }

    public DateTime UploadedDate { get; set; }

    public string? Description { get; set; }

    public int? UploadedUserId { get; set; }
    public int OrderId { get; set; }

    public DocumentType DocumentType { get; set; }

    public Order Order { get; set; }
}
