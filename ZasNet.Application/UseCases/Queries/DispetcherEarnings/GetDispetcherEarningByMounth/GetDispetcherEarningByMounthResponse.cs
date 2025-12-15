using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Ответ с данными о заработке диспетчера
/// </summary>
public class GetDispetcherEarningByMounthResponse
{
    /// <summary>
    /// ID записи о заработке
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID заявки
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Клиент заявки
    /// </summary>
    public string Client { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала заявки
    /// </summary>
    public DateTime OrderDateStart { get; set; }

    /// <summary>
    /// Дата окончания заявки
    /// </summary>
    public DateTime OrderDateEnd { get; set; }

    /// <summary>
    /// Диспетчер
    /// </summary>
    public EmployeeDto Dispetcher { get; set; } = new();

    /// <summary>
    /// Процент для диспетчера
    /// </summary>
    public decimal DispetcherPrecent { get; set; }

    /// <summary>
    /// Описание процента
    /// </summary>
    public string? PrecentDispetcherDescription { get; set; }

    /// <summary>
    /// Заработок диспетчера
    /// </summary>
    public decimal DispetcherEarning { get; set; }

    /// <summary>
    /// Общая стоимость заявки
    /// </summary>
    public decimal OrderTotalPrice { get; set; }
}
