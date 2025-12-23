using MediatR;
using ZasNet.Application.CommonDtos;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetCarEarningByPeriod;

/// <summary>
/// Запрос на получение прибыли машин по периодам (день, месяц, год)
/// </summary>
public class GetCarEarningByPeriodRequest : IRequest<List<CarEarningByPeriodDto>>
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

    /// <summary>
    /// Тип периода для группировки
    /// </summary>
    public GroupPeriod GroupPeriod { get; set; } = GroupPeriod.Day;
}

