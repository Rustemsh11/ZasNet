using Microsoft.Extensions.Logging;
using System.Text;
using ZasNet.Domain;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Helpers;
using ZasNet.Domain.Telegram;

namespace ZasNet.Application.Services.Telegram;

public class OrderNotificationService(ITelegramBotAnswerService telegramBotAnswer, ILoggerManager logger) : IOrderNotificationService
{
    public async Task NotifyOrderCreatedAsync(Order order, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            var serviesText = new StringBuilder();
            var buttons = new List<Button>();

            for (int i = 0; i < order.OrderServices.Count; i++)
            {
                serviesText.AppendLine();

                var service = order.OrderServices.ElementAt(i);

                // Заголовок услуги
                serviesText.AppendLine($"   🔧 Услуга {i + 1}: {service.Service.Name}");
                serviesText.AppendLine($"       💵 Цена: {service.Price:0.##} • 📦 Объем: {service.TotalVolume}");
                serviesText.AppendLine($"       🧮 Итого: {service.PriceTotal:0.##}");

                // Сотрудники
                var serviceEmployees = service.OrderServiceEmployees.Distinct().ToList();
                serviesText.AppendLine("    👷 Сотрудники:");
                for (int k = 0; k < serviceEmployees.Count; k++)
                {
                    if (serviceEmployees[k].Employee.ChatId == chatId)
                    {
                        buttons.Add(new Button() { Text = $"Подтвердить услугу {i+1}", CallbackData = $"approveorderservice:{serviceEmployees[k].Id}" });
                        buttons.Add(new Button() { Text = $"Отказаться от услуги {i+1}", CallbackData = $"rejectorderservice:{serviceEmployees[k].Id}" });
                    }

                    if (serviceEmployees[k].Employee.Id == Constants.UnknowingEmployeeId)
                    {
                        serviesText.AppendLine($"       {k + 1}  🆓 Свободно");
                    }
                    else
                    {
                        serviesText.AppendLine($"       {k + 1}  ❓ {serviceEmployees[k].Employee.Name}");
                    }
                }

                // Машины
                var orderServiceCars = service.OrderServiceCars.ToList();
                if (orderServiceCars.Count == 0)
                {
                    serviesText.AppendLine("    🚗 Машины: пока не назначены");
                }
                else
                {
                    serviesText.AppendLine("    🚗 Машины:");
                    foreach (var car in orderServiceCars)
                    {
                        serviesText.AppendLine($"       ❓ • {car.Car.CarModel.Name} ({car.Car.Number})");
                    }
                }

                // Разделитель между услугами
                serviesText.AppendLine("━━━━━━━━━━━━━━━━━━━━");
            }


            var sb = new StringBuilder();
            sb.AppendLine("🆕 Новая заявка");
            sb.AppendLine($"🧑 Клиент: {order.Client}");
            sb.AppendLine($"📍 Адрес: {order.AddressCity}, {order.AddressStreet} {order.AddressNumber}");
            sb.AppendLine($"🗓️ Дата: {order.DateStart:dd.MM.yyyy HH:mm} - {order.DateEnd:dd.MM.yyyy HH:mm}");
            sb.AppendLine();
            sb.AppendLine("🧾 Услуги:");
            sb.AppendLine(serviesText.ToString());
            sb.AppendLine($"💰 Общая сумма: {order.OrderPriceAmount:0.##}");
            sb.AppendLine($"💳 Оплата: {EnumsToStringConverter.GetPaymentTypeDescription(order.PaymentType)}");
            if (order.PaymentType == PaymentType.Cash)
            {
                sb.AppendLine("‼️ Необходимо забрать оплату после выполнения!");
            }

            if (!string.IsNullOrWhiteSpace(order.Description))
            {
                sb.AppendLine();
                sb.AppendLine("📝 Комментарий:");
                sb.AppendLine(order.Description);
            }

            await telegramBotAnswer.SendMessageAsync(chatId, sb.ToString(), buttons, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send Telegram notification for order {order.Id}. Ex: {ex.Message}");
        }
    }

    public async Task NotifyOrderCreatedAsync(long chatId, CancellationToken cancellationToken)
    {
        await telegramBotAnswer.SendMessageAsync(chatId, "Появилась новая заявка", cancellationToken);
    }
}

