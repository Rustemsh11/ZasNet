using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram;

public interface IUserSessionCache
{
	bool TryGet(long chatId, out EditOrderSession session);
	void Set(EditOrderSession session, TimeSpan ttl);
	void Invalidate(long chatId);
}
