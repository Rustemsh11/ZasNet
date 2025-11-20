using ZasNet.Application.Services;
using Microsoft.Extensions.Options;

namespace ZasNet.Infrastruture.Services;

public class TelegramValidate(IOptions<TelegramSettings> telegramOptions) : ITelegramValidate
{
    private readonly TelegramSettings _settings = telegramOptions.Value;

    public bool IsEnabled() => _settings.IsEnabled;

    public bool IsSecretValid(string secret) =>
        string.Equals(secret, _settings.WebhookSecret, StringComparison.Ordinal);

    public bool IsUserAllowed(long userId) =>
        _settings.ManagerUserIds.Count == 0 || _settings.ManagerUserIds.Contains(userId);
}

