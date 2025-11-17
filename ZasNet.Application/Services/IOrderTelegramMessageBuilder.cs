using Telegram.Bot.Types.ReplyMarkups;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.Services;

public interface IOrderTelegramMessageBuilder
{
    string BuildOrderMessage(Order order);

    InlineKeyboardMarkup BuildStatusKeyboard(int orderId, OrderStatus currentStatus);
}

