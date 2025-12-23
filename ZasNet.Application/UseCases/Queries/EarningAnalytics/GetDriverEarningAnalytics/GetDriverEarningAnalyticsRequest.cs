using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDriverEarningAnalytics;

/// <summary>
/// Запрос на получение аналитики заработков по водителям
/// </summary>
public class GetDriverEarningAnalyticsRequest : IRequest<List<DriverEarningAnalyticsDto>>
{
    /// <summary>
    /// Дата начала периода (обязательная)
    /// </summary>
    public DateTime DateFrom { get; set; }

    /// <summary>
    /// Дата окончания периода (обязательная)
    /// </summary>
    public DateTime DateTo { get; set; }

    /// <summary>
    /// Фильтр по ID водителей (опционально)
    /// </summary>
    public List<int>? DriverIds { get; set; }
}

