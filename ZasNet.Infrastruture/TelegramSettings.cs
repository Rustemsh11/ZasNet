namespace ZasNet.Infrastruture;

public class TelegramSettings
{
    public string BotToken { get; set; } = string.Empty;

    public string ChannelId { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public List<long> ManagerUserIds { get; set; } = [];

    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(BotToken)
        && !string.IsNullOrWhiteSpace(ChannelId);
        //&& !string.IsNullOrWhiteSpace(WebhookSecret);
}

