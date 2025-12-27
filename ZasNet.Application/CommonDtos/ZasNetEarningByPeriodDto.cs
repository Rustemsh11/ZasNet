using ZasNet.Domain.Enums;

namespace ZasNet.Application.CommonDtos;

/// <summary>
/// Прибыль компании за период
/// </summary>
public class ZasNetEarningByPeriodDto
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
    /// Общая прибыль за период (OrderPriceAmount)
    /// </summary>
    public decimal CommonEarning { get; set; }
    
    /// <summary>
    /// Прибыль Алмаза за период (OrderPriceAmount)
    /// </summary>
    public decimal AlmazEarning { get; set; }
    
    /// <summary>
    /// Общая прибыль c учетом налога период (OrderPriceAmount)
    /// </summary>
    public decimal CommonEarningWithVat { get; set; }
    
    /// <summary>
    /// Прибыль Алмаза c учетом налога за период (OrderPriceAmount)
    /// </summary>
    public decimal AlmazEarningWithVat { get; set; }

}

