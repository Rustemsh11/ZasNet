using MediatR;
using ZasNet.Application.CommonDtos;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDriverEarningByPeriod;

/// <summary>
/// Запрос на получение прибыли водителей по периодам (день, месяц, год)
/// </summary>
public class GetDriverEarningByPeriodRequest : IRequest<List<DriverEarningByPeriodDto>>
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

    /// <summary>
    /// Тип периода для группировки
    /// </summary>
    public GroupPeriod GroupPeriod { get; set; } = GroupPeriod.Day;
}

