using ZasNet.Domain.Enums;
using ZasNet.Domain.Telegram;

namespace ZasNet.Domain.Interfaces;

public interface IMessageTypeResolver
{
    TelegramMessageType ResolveMessageType(TelegramUpdate update);
}
