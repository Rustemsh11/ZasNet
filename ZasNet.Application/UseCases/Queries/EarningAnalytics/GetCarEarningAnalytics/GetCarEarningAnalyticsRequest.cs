using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetCarEarningAnalytics;

/// <summary>
/// Запрос на получение аналитики заработков по машинам
/// </summary>
public class GetCarEarningAnalyticsRequest : IRequest<List<CarEarningAnalyticsDto>>
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
    /// Фильтр по ID машин (опционально)
    /// </summary>
    public List<int>? CarIds { get; set; }
}

