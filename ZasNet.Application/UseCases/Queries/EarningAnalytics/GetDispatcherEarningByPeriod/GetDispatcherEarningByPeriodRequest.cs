using MediatR;
using ZasNet.Application.CommonDtos;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDispatcherEarningByPeriod;

/// <summary>
/// Запрос на получение прибыли диспетчеров по периодам (день, месяц, год)
/// </summary>
public class GetDispatcherEarningByPeriodRequest : IRequest<List<DispatcherEarningByPeriodDto>>
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

    /// <summary>
    /// Тип периода для группировки
    /// </summary>
    public GroupPeriod GroupPeriod { get; set; } = GroupPeriod.Day;
}

