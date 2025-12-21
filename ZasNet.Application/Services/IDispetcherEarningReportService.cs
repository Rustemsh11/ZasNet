using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.Services;

/// <summary>
/// Сервис для генерации отчетов по заработкам диспетчеров
/// </summary>
public interface IDispetcherEarningReportService
{
    /// <summary>
    /// Генерирует PDF-отчет по заработкам диспетчера на основе шаблона Excel
    /// </summary>
    /// <param name="data">Список данных для отчета</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Массив байтов PDF-документа</returns>
    Task<byte[]> GenerateReportPdfAsync(List<DispetcherEarningByFilterDto> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Генерирует имя файла для отчета
    /// </summary>
    /// <param name="dispetcherName">Имя диспетчера</param>
    /// <param name="month">Месяц</param>
    /// <param name="year">Год</param>
    /// <returns>Имя файла</returns>
    string GenerateFileName(string dispetcherName, int month, int year);
}

