namespace ZasNet.Domain.Telegram;

public class TelegramPhoto
{
    public string FileId { get; set; } = string.Empty;
    public string FileUniqueId { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public long? FileSize { get; set; }
}
