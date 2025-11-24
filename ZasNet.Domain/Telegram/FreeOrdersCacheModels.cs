namespace ZasNet.Domain.Telegram;

public class Button
{
    public string Text { get; set; } = string.Empty;
    public string CallbackData { get; set; } = string.Empty;
}

public class CachedOrderPage
{
    public string MessageText { get; set; } = string.Empty;
    public List<Button> Buttons { get; set; } = new();
}

public class CachedFreeOrders
{
    public long ChatId { get; set; }
    public DateTimeOffset ExpireAt { get; set; }
    public List<CachedOrderPage> Pages { get; set; } = new();
}

