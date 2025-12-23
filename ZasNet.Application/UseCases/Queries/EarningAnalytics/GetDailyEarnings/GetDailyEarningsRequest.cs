using MediatR;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDailyEarnings;

/// <summary>
/// Запрос на получение заработка по дням
/// </summary>
public class GetDailyEarningsRequest : IRequest<Dictionary<DateTime, decimal>>
{
    /// <summary>
    /// Дата начала периода (обязательная)
    /// </summary>
    public DateTime DateFrom { get; set; }

    /// <summary>
    /// Дата окончания периода (обязательная)
    /// </summary>
    public DateTime DateTo { get; set; }
}

