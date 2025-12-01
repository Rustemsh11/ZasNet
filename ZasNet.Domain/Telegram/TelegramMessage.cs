namespace ZasNet.Domain.Telegram;

public class TelegramMessage
{
    public long MessageId { get; set; }
    public TelegramUser? From { get; set; }
    public DateTime Date { get; set; }
    public string? Text { get; set; }
    public TelegramPhoto[]? Photo { get; set; }

    public TelegramDocument? Document { get; set; }
}
