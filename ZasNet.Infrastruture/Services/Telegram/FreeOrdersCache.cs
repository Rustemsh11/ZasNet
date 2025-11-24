using System.Collections.Concurrent;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Telegram;

namespace ZasNet.Infrastruture.Services.Telegram;

public class FreeOrdersCache : IFreeOrdersCache
{
    private readonly ConcurrentDictionary<long, CachedFreeOrders> storage = new();

    public void Set(long chatId, List<CachedOrderPage> pages, TimeSpan ttl)
    {
        var entry = new CachedFreeOrders
        {
            ChatId = chatId,
            Pages = pages,
            ExpireAt = DateTimeOffset.UtcNow.Add(ttl)
        };
        storage.AddOrUpdate(chatId, entry, (_, __) => entry);
    }

    public bool TryGet(long chatId, out List<CachedOrderPage> pages)
    {
        pages = new List<CachedOrderPage>();
        if (storage.TryGetValue(chatId, out var entry))
        {
            if (entry.ExpireAt > DateTimeOffset.UtcNow)
            {
                pages = entry.Pages;
                return true;
            }
            storage.TryRemove(chatId, out _);
        }

        return false;
    }

    public void Invalidate(long chatId)
    {
        storage.TryRemove(chatId, out _);
    }
}

