namespace ZasNet.Domain.Telegram;

public class TelegramUser
{
    public long Id { get; set; }

    public long ChatId { get; set; }
    
    public string? Username { get; set; }
}
