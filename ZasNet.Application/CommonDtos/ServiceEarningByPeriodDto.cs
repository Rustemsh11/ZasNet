using ZasNet.Domain.Enums;

namespace ZasNet.Application.CommonDtos;

/// <summary>
/// Прибыль услуги за период
/// </summary>
public class ServiceEarningByPeriodDto
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
    /// Название услуги
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// ID услуги
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Общая прибыль за период
    /// </summary>
    public decimal TotalEarning { get; set; }
}

