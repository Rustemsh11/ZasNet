using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetCarEarningAnalytics;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetServiceEarningAnalytics;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDriverEarningAnalytics;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDispatcherEarningAnalytics;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetZasNetEarning;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetServiceEarningByPeriod;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetCarEarningByPeriod;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDriverEarningByPeriod;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDispatcherEarningByPeriod;

namespace ZasNet.WebApi.Controllers;

/// <summary>
/// Контроллер для аналитики заработков
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class EarningAnalyticsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Получить аналитику заработков по машинам за период
    /// </summary>
    /// <remarks>
    /// Возвращает агрегированную аналитику заработков по машинам:
    /// - Общий заработок водителей, работавших на каждой машине
    /// - Количество заявок и услуг для каждой машины
    /// </remarks>
    [HttpGet("cars")]
    public async Task<List<CarEarningAnalyticsDto>> GetCarEarningAnalytics(
        [FromQuery] GetCarEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить аналитику заработков по услугам за период
    /// </summary>
    /// <remarks>
    /// Возвращает агрегированную аналитику заработков по услугам:
    /// - Общий заработок сотрудников за каждую услугу
    /// - Количество заявок и выполненных услуг
    /// - Общая стоимость услуг
    /// </remarks>
    [HttpGet("services")]
    public async Task<List<ServiceEarningAnalyticsDto>> GetServiceEarningAnalytics(
        [FromQuery] GetServiceEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить аналитику заработков по водителям за период
    /// </summary>
    /// <remarks>
    /// Возвращает агрегированную аналитику заработков по водителям:
    /// - Общий заработок каждого водителя
    /// - Количество заявок и услуг для каждого водителя
    /// </remarks>
    [HttpGet("drivers")]
    public async Task<List<DriverEarningAnalyticsDto>> GetDriverEarningAnalytics(
        [FromQuery] GetDriverEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить аналитику заработков по диспетчерам за период
    /// </summary>
    /// <remarks>
    /// Возвращает агрегированную аналитику заработков по диспетчерам:
    /// - Общий заработок каждого диспетчера
    /// - Количество заявок для каждого диспетчера
    /// - Общая стоимость заявок
    /// </remarks>
    [HttpGet("dispatchers")]
    public async Task<List<DispatcherEarningAnalyticsDto>> GetDispatcherEarningAnalytics(
        [FromQuery] GetDispatcherEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить прибыль компании по периодам (день, месяц, год)
    /// </summary>
    /// <remarks>
    /// Возвращает список прибыли компании, сгруппированный по указанному периоду:
    /// - Day (1): группировка по дням
    /// - Month (2): группировка по месяцам
    /// - Year (3): группировка по годам
    /// 
    /// Параметр GroupPeriod определяет тип группировки.
    /// Все периоды в диапазоне включены, даже если прибыль равна нулю.
    /// </remarks>
    [HttpGet("zasnet-earning")]
    public async Task<List<ZasNetEarningByPeriodDto>> GetZasNetEarning(
        [FromQuery] GetZasNetEarningRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить прибыль каждой услуги по периодам (день, месяц, год)
    /// </summary>
    /// <remarks>
    /// Возвращает список прибыли услуг, сгруппированный по указанному периоду:
    /// - Day (1): группировка по дням
    /// - Month (2): группировка по месяцам
    /// - Year (3): группировка по годам
    /// 
    /// Параметр GroupPeriod определяет тип группировки.
    /// </remarks>
    [HttpGet("service-earnings-by-period")]
    public async Task<List<ServiceEarningByPeriodDto>> GetServiceEarningByPeriod(
        [FromQuery] GetServiceEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить прибыль каждой машины по периодам (день, месяц, год)
    /// </summary>
    /// <remarks>
    /// Возвращает список прибыли машин, сгруппированный по указанному периоду:
    /// - Day (1): группировка по дням
    /// - Month (2): группировка по месяцам
    /// - Year (3): группировка по годам
    /// 
    /// Параметр GroupPeriod определяет тип группировки.
    /// </remarks>
    [HttpGet("car-earnings-by-period")]
    public async Task<List<CarEarningByPeriodDto>> GetCarEarningByPeriod(
        [FromQuery] GetCarEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить прибыль каждого водителя по периодам (день, месяц, год)
    /// </summary>
    /// <remarks>
    /// Возвращает список прибыли водителей, сгруппированный по указанному периоду:
    /// - Day (1): группировка по дням
    /// - Month (2): группировка по месяцам
    /// - Year (3): группировка по годам
    /// 
    /// Параметр GroupPeriod определяет тип группировки.
    /// </remarks>
    [HttpGet("driver-earnings-by-period")]
    public async Task<List<DriverEarningByPeriodDto>> GetDriverEarningByPeriod(
        [FromQuery] GetDriverEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Получить прибыль каждого диспетчера по периодам (день, месяц, год)
    /// </summary>
    /// <remarks>
    /// Возвращает список прибыли диспетчеров, сгруппированный по указанному периоду:
    /// - Day (1): группировка по дням
    /// - Month (2): группировка по месяцам
    /// - Year (3): группировка по годам
    /// 
    /// Параметр GroupPeriod определяет тип группировки.
    /// </remarks>
    [HttpGet("dispatcher-earnings-by-period")]
    public async Task<List<DispatcherEarningByPeriodDto>> GetDispatcherEarningByPeriod(
        [FromQuery] GetDispatcherEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }
}

