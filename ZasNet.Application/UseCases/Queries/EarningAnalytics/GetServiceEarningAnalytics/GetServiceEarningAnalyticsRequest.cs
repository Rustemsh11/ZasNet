using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetServiceEarningAnalytics;

/// <summary>
/// Запрос на получение аналитики заработков по услугам
/// </summary>
public class GetServiceEarningAnalyticsRequest : IRequest<List<ServiceEarningAnalyticsDto>>
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
    /// Фильтр по ID услуг (опционально)
    /// </summary>
    public List<int>? ServiceIds { get; set; }
}

