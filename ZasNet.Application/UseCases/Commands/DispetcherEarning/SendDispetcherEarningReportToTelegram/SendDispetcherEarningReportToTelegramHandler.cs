using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Application.Services.Telegram;

namespace ZasNet.Application.UseCases.Commands.DispetcherEarning.SendDispetcherEarningReportToTelegram;

/// <summary>
/// Обработчик команды для отправки отчета по заработку диспетчера в Telegram
/// </summary>
public class SendDispetcherEarningReportToTelegramHandler(
    IRepositoryManager repositoryManager,
    IDispetcherEarningReportService reportService,
    ITelegramBotAnswerService telegramBotAnswerService)
    : IRequestHandler<SendDispetcherEarningReportToTelegramCommand>
{
    public async Task Handle(
        SendDispetcherEarningReportToTelegramCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Data == null || !request.Data.Any())
        {
            throw new ArgumentException("Список данных для отчета не может быть пустым");
        }

        var firstRecord = request.Data.First();
        var dispetcherId = firstRecord.Dispetcher.Id;

        // Получаем информацию о диспетчере из БД для получения ChatId
        var employee = await repositoryManager.EmployeeRepository
            .FindByCondition(e => e.Id == dispetcherId, false)
            .FirstOrDefaultAsync(cancellationToken);

        if (employee == null)
        {
            throw new InvalidOperationException($"Диспетчер с ID {dispetcherId} не найден");
        }

        // Проверяем наличие ChatId
        if (employee.ChatId == null)
        {
            throw new InvalidOperationException(
                $"У диспетчера {employee.Name} не привязан Telegram аккаунт");
        }

        // Генерируем PDF отчет
        var pdfBytes = await reportService.GenerateReportPdfAsync(request.Data, cancellationToken);

        // Определяем период
        var minDate = request.Data.Min(d => d.OrderDateStart);
        var maxDate = request.Data.Max(d => d.OrderDateEnd);
        var totalEarning = request.Data.Sum(d => d.DispetcherEarning);

        // Формируем имя файла
        var fileName = reportService.GenerateFileName(employee.Name, minDate.Month, minDate.Year);

        // Формируем сообщение
        var caption = $"📊 Отчет по заработной плате диспетчера\n" +
                     $"Диспетчер: {employee.Name}\n" +
                     $"Период: {minDate:dd.MM.yyyy} - {maxDate:dd.MM.yyyy}\n" +
                     $"Количество заказов: {request.Data.Count}\n" +
                     $"Итоговый заработок: {totalEarning:N2} ₽";

        // Отправляем отчет в Telegram
        await telegramBotAnswerService.SendDocumentAsync(
            employee.ChatId.Value,
            pdfBytes,
            fileName,
            caption,
            cancellationToken);
    }
}

