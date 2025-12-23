using ZasNet.Domain.Enums;

namespace ZasNet.Application.CommonDtos;

/// <summary>
/// Прибыль машины за период
/// </summary>
public class CarEarningByPeriodDto
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
    /// Номер машины
    /// </summary>
    public string CarNumber { get; set; } = string.Empty;

    public string CarModel { get; set; } = string.Empty;

    /// <summary>
    /// ID машины
    /// </summary>
    public int CarId { get; set; }

    /// <summary>
    /// Общая прибыль за период
    /// </summary>
    public decimal TotalEarning { get; set; }
}

