using ZasNet.Domain.Enums;

namespace ZasNet.Application.CommonDtos;

/// <summary>
/// Прибыль водителя за период
/// </summary>
public class DriverEarningByPeriodDto
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
    /// Имя водителя
    /// </summary>
    public string DriverName { get; set; } = string.Empty;

    /// <summary>
    /// ID водителя
    /// </summary>
    public int DriverId { get; set; }

    /// <summary>
    /// Общая прибыль за период
    /// </summary>
    public decimal TotalEarning { get; set; }
}

