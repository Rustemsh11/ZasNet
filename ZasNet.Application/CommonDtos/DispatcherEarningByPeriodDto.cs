using ZasNet.Domain.Enums;

namespace ZasNet.Application.CommonDtos;

/// <summary>
/// Прибыль диспетчера за период
/// </summary>
public class DispatcherEarningByPeriodDto
{
    /// <summary>
    /// Период (дата для дня, первый день месяца для месяца, первый день года для года)
    /// </summary>
    public DateTime Period { get; set; }

    /// <summary>
    /// Тип периода для группировки
    /// </summary>
    public GroupPeriod GroupPeriod { get; set; }

    /// <summary>
    /// Имя диспетчера
    /// </summary>
    public string DispatcherName { get; set; } = string.Empty;

    /// <summary>
    /// ID диспетчера
    /// </summary>
    public int DispatcherId { get; set; }

    /// <summary>
    /// Общая прибыль за период
    /// </summary>
    public decimal TotalEarning { get; set; }
}

