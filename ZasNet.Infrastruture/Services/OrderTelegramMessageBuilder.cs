using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Infrastruture.Services;

public class OrderTelegramMessageBuilder : IOrderTelegramMessageBuilder
{
    private static readonly OrderStatus[] StatusOrder =
    [
        OrderStatus.Created,
        OrderStatus.Processing,
        OrderStatus.Finished,
        OrderStatus.CreatedInvoice,
        OrderStatus.AwaitingPayment,
        OrderStatus.Closed
    ];

    public string BuildOrderMessage(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Order #{order.Id} • {order.Status}");
        sb.AppendLine($"Client: {order.Client}");
        sb.AppendLine($"Address: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
        sb.AppendLine($"Date: {order.Date:dd.MM.yyyy HH:mm}");
        sb.AppendLine($"Amount: {order.OrderPriceAmount:0.##}");
        sb.AppendLine($"Payment: {order.PaymentType}");
        sb.AppendLine($"Client type: {order.ClientType}");

        if (!string.IsNullOrWhiteSpace(order.Description))
        {
            sb.AppendLine();
            sb.AppendLine(order.Description);
        }

        return sb.ToString();
    }

    public InlineKeyboardMarkup BuildStatusKeyboard(int orderId, OrderStatus currentStatus)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        var currentRow = new List<InlineKeyboardButton>(2);

        foreach (var status in StatusOrder)
        {
            var label = status == currentStatus ? $"• {status}" : status.ToString();
            currentRow.Add(InlineKeyboardButton.WithCallbackData(label, BuildCallback(orderId, status)));

            if (currentRow.Count == 2)
            {
                buttons.Add(currentRow.ToArray());
                currentRow = new List<InlineKeyboardButton>(2);
            }
        }

        if (currentRow.Count > 0)
        {
            buttons.Add(currentRow.ToArray());
        }

        return new InlineKeyboardMarkup(buttons);
    }

    private static string BuildCallback(int orderId, OrderStatus status) =>
        $"order:{orderId}:status:{(int)status}";
}

