using MediatR;
using ZasNet.Application.CommonDtos;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetServiceEarningByPeriod;

/// <summary>
/// Запрос на получение прибыли услуг по периодам (день, месяц, год)
/// </summary>
public class GetServiceEarningByPeriodRequest : IRequest<List<ServiceEarningByPeriodDto>>
{
    /// <summary>
    /// Дата начала периода (обязательная)
    /// </summary>
    public DateTime DateFrom { get; set; }

    /// <summary>
    /// Дата окончания периода (обязательная)
    /// </summary>
    public DateTime DateTo { get; set; }


    public List<int>? ServiceIds { get; set; }

    /// <summary>
    /// Тип периода для группировки
    /// </summary>
    public GroupPeriod GroupPeriod { get; set; } = GroupPeriod.Day;
}

