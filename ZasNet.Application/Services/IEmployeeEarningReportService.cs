using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.Services;

/// <summary>
/// Сервис для генерации отчетов по заработкам сотрудников
/// </summary>
public interface IEmployeeEarningReportService
{
    /// <summary>
    /// Генерирует PDF-отчет по заработкам сотрудника на основе шаблона Excel
    /// </summary>
    /// <param name="data">Список данных для отчета</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Массив байтов PDF-документа</returns>
    Task<byte[]> GenerateReportPdfAsync(List<EmployeeEarningByFilterDto> data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Генерирует имя файла для отчета
    /// </summary>
    /// <param name="employeeName">Имя сотрудника</param>
    /// <param name="month">Месяц</param>
    /// <param name="year">Год</param>
    /// <returns>Имя файла</returns>
    string GenerateFileName(string employeeName, int month, int year);
}

