using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDispatcherEarningAnalytics;

/// <summary>
/// Запрос на получение аналитики заработков по диспетчерам
/// </summary>
public class GetDispatcherEarningAnalyticsRequest : IRequest<List<DispatcherEarningAnalyticsDto>>
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
    /// Фильтр по ID диспетчеров (опционально)
    /// </summary>
    public List<int>? DispatcherIds { get; set; }
}

