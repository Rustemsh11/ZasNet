namespace ZasNet.Domain.Telegram;

public class TelegramCallbackQuery
{
    public string Id { get; set; } = string.Empty;
    public TelegramUser? From { get; set; }
    public TelegramMessage? Message { get; set; }
    public string? Data { get; set; }
}
