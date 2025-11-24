using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram;

public interface IFreeOrdersCache
{
    void Set(long chatId, List<CachedOrderPage> pages, TimeSpan ttl);
    bool TryGet(long chatId, out List<CachedOrderPage> pages);
    void Invalidate(long chatId);
}

