using Telegram.Bot.Types.Enums;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Interfaces;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram;

public class MessageTypeResolver : IMessageTypeResolver
{
    public TelegramMessageType ResolveMessageType(TelegramUpdate update)
    {
        if (update.CallbackQuery != null)
        {
            //// Проверяем, является ли это подтверждением заявки
            //if (update.CallbackQuery.Data?.StartsWith("confirm_", StringComparison.OrdinalIgnoreCase) == true)
            //{
            //    return MessageType.ApplicationConfirmation;
            //}

            //return MessageType.ButtonClick;
        }

        // Проверка на текстовое сообщение
        if (update.Message != null)
        {
            // Проверка на команду /start
            if (update.Message.Text?.Equals("/start", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TelegramMessageType.Start;
            }

            // Проверка на наличие фото
            if (update.Message.Photo != null && update.Message.Photo.Length > 0)
            {
                //return MessageType.PhotoMessage;
            }
        }

        return TelegramMessageType.None;
    }
}
