using MediatR;
using ZasNet.Application.CommonDtos;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetZasNetEarning;

/// <summary>
/// Запрос на получение прибыли компании по периодам (день, месяц, год)
/// </summary>
public class GetZasNetEarningRequest : IRequest<List<ZasNetEarningByPeriodDto>>
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
    /// Тип периода для группировки
    /// </summary>
    public GroupPeriod GroupPeriod { get; set; } = GroupPeriod.Day;
}

