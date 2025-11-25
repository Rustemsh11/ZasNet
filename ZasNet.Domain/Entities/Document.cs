using ZasNet.Domain.Enums;

namespace ZasNet.Domain.Entities;

public class Document : LockedItemBase
{
    public string Name { get; set; }

    public string Extension { get; set; }

    public string Path { get; set; }

    public DateTime UploadedDate { get; set; }

    public string? Description { get; set; }

    public int? UploadedUserId { get; set; }
    public int OrderId { get; set; }

    public DocumentType DocumentType { get; set; }

    public Order Order { get; set; }
}
