namespace ZasNet.Domain.Telegram;

public enum EditStep
{
	None = 0,
	SelectingService = 1,
	AwaitingPrice = 2,
	AwaitingVolume = 3,
	PhotoUploading = 4
}

public class EditOrderSession
{
	public long ChatId { get; set; }
	public int OrderId { get; set; }
	public int? OrderServiceId { get; set; }
	public EditStep Step { get; set; } = EditStep.None;
	public decimal? TempPrice { get; set; }
	public double? TempVolume { get; set; }
	public List<string> PhotoFileIds { get; set; } = new();
	public List<PendingDocument> PendingDocuments { get; set; } = new();
	public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.Now;
}
