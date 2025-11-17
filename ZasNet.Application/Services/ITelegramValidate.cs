namespace ZasNet.Application.Services;

public interface ITelegramValidate
{
    bool IsEnabled();

    bool IsSecretValid(string secret);

    bool IsUserAllowed(long userId);
}

