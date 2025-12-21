using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.Services;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.EmployeeEarningReport;

/// <summary>
/// Обработчик запроса на скачивание отчета по заработку сотрудника
/// </summary>
public class DownloadEmployeeEarningReportHandler(
    IEmployeeEarningReportService reportService)
    : IRequestHandler<DownloadEmployeeEarningReportRequest, FileContentResult>
{
    public async Task<FileContentResult> Handle(
        DownloadEmployeeEarningReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Data == null || !request.Data.Any())
        {
            throw new ArgumentException("Список данных для отчета не может быть пустым");
        }

        var firstRecord = request.Data.First();
        
        // Генерируем PDF отчет
        var pdfBytes = await reportService.GenerateReportPdfAsync(request.Data, cancellationToken);

        // Определяем период для имени файла
        var minDate = request.Data.Min(d => d.OrderDateStart);
        var maxDate = request.Data.Max(d => d.OrderDateEnd);
        
        // Формируем имя файла
        var fileName = reportService.GenerateFileName(
            firstRecord.Employee.Name, 
            minDate.Month, 
            minDate.Year);

        // Возвращаем файл для скачивания
        return new FileContentResult(pdfBytes, "application/pdf")
        {
            FileDownloadName = fileName
        };
    }
}

