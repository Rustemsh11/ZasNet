using System.Collections.Concurrent;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Telegram;

namespace ZasNet.Infrastruture.Services.Telegram;

public class UserSessionCache : IUserSessionCache
{
	private readonly ConcurrentDictionary<long, (EditOrderSession Session, DateTimeOffset ExpireAt)> storage = new();

	public bool TryGet(long chatId, out EditOrderSession session)
	{
		session = default!;
		if (storage.TryGetValue(chatId, out var entry))
		{
			if (entry.ExpireAt > DateTimeOffset.Now)
			{
				session = entry.Session;
				return true;
			}

			storage.TryRemove(chatId, out _);
		}
		return false;
	}

	public void Set(EditOrderSession session, TimeSpan ttl)
	{
		var item = (session, DateTimeOffset.Now.Add(ttl));
		storage.AddOrUpdate(session.ChatId, item, (_, __) => item);
	}

	public void Invalidate(long chatId)
	{
		storage.TryRemove(chatId, out _);
	}
}


