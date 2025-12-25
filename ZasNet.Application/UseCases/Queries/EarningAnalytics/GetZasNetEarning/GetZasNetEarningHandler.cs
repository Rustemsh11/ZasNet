using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetZasNetEarning;

/// <summary>
/// Обработчик запроса на получение прибыли компании по периодам
/// </summary>
public class GetZasNetEarningHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetZasNetEarningRequest, List<ZasNetEarningByPeriodDto>>
{
    public async Task<List<ZasNetEarningByPeriodDto>> Handle(
        GetZasNetEarningRequest request,
        CancellationToken cancellationToken)
    {
        // Получаем все заявки за период
        var orders = await repositoryManager.OrderRepository
            .FindAll(false)
            .Where(o => o.DateStart >= request.DateFrom
                     && o.DateStart <= request.DateTo)
            .ToListAsync(cancellationToken);

        // Группировка в зависимости от выбранного типа периода
        return request.GroupPeriod switch
        {
            GroupPeriod.Day => GetDailyEarnings(orders, request.DateFrom, request.DateTo),
            GroupPeriod.Month => GetMonthlyEarnings(orders, request.DateFrom, request.DateTo),
            GroupPeriod.Year => GetYearlyEarnings(orders, request.DateFrom, request.DateTo),
            _ => throw new ArgumentException($"Неизвестный тип периода: {request.GroupPeriod}")
        };
    }

    /// <summary>
    /// Группировка по дням
    /// </summary>
    private List<ZasNetEarningByPeriodDto> GetDailyEarnings(
        List<ZasNet.Domain.Entities.Order> orders,
        DateTime dateFrom,
        DateTime dateTo)
    {
        var earnings = orders
            .GroupBy(o => o.DateStart.Date)
            .Select(g => new ZasNetEarningByPeriodDto
            {
                Period = g.Key,
                GroupPeriod = GroupPeriod.Day,
                TotalEarning = g.Sum(o => o.OrderPriceAmount)
            })
            .OrderBy(r => r.Period)
            .ToList();

        // Заполняем дни без заработка нулями для полного диапазона
        return FillMissingPeriods(earnings, GroupPeriod.Day, dateFrom, dateTo);
    }

    /// <summary>
    /// Группировка по месяцам
    /// </summary>
    private List<ZasNetEarningByPeriodDto> GetMonthlyEarnings(
        List<ZasNet.Domain.Entities.Order> orders,
        DateTime dateFrom,
        DateTime dateTo)
    {
        var earnings = orders
            .GroupBy(o => new
            {
                Year = o.DateStart.Year,
                Month = o.DateStart.Month,
                FirstDayOfMonth = new DateTime(o.DateStart.Year, o.DateStart.Month, 1)
            })
            .Select(g => new ZasNetEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfMonth,
                GroupPeriod = GroupPeriod.Month,
                TotalEarning = g.Sum(o => o.OrderPriceAmount)
            })
            .OrderBy(r => r.Period)
            .ToList();

        // Заполняем месяцы без заработка нулями для полного диапазона
        var firstDayOfMonth = new DateTime(dateFrom.Year, dateFrom.Month, 1);
        var lastDayOfMonth = new DateTime(dateTo.Year, dateTo.Month, 1);
        return FillMissingPeriods(earnings, GroupPeriod.Month, firstDayOfMonth, lastDayOfMonth);
    }

    /// <summary>
    /// Группировка по годам
    /// </summary>
    private List<ZasNetEarningByPeriodDto> GetYearlyEarnings(
        List<ZasNet.Domain.Entities.Order> orders,
        DateTime dateFrom,
        DateTime dateTo)
    {
        var earnings = orders
            .GroupBy(o => new
            {
                Year = o.DateStart.Year,
                FirstDayOfYear = new DateTime(o.DateStart.Year, 1, 1)
            })
            .Select(g => new ZasNetEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfYear,
                GroupPeriod = GroupPeriod.Year,
                TotalEarning = g.Sum(o => o.OrderPriceAmount)
            })
            .OrderBy(r => r.Period)
            .ToList();

        // Заполняем годы без заработка нулями для полного диапазона
        var firstDayOfYear = new DateTime(dateFrom.Year, 1, 1);
        var lastDayOfYear = new DateTime(dateTo.Year, 1, 1);
        return FillMissingPeriods(earnings, GroupPeriod.Year, firstDayOfYear, lastDayOfYear);
    }

    /// <summary>
    /// Заполняет пропущенные периоды нулями
    /// </summary>
    private List<ZasNetEarningByPeriodDto> FillMissingPeriods(
        List<ZasNetEarningByPeriodDto> earnings,
        GroupPeriod groupPeriod,
        DateTime dateFrom,
        DateTime dateTo)
    {
        var result = new List<ZasNetEarningByPeriodDto>();
        var earningsDict = earnings.ToDictionary(e => e.Period, e => e.TotalEarning);

        var currentPeriod = dateFrom.Date;

        // Нормализуем начальный период в зависимости от типа группировки
        if (groupPeriod == GroupPeriod.Month)
        {
            currentPeriod = new DateTime(currentPeriod.Year, currentPeriod.Month, 1);
        }
        else if (groupPeriod == GroupPeriod.Year)
        {
            currentPeriod = new DateTime(currentPeriod.Year, 1, 1);
        }

        // Нормализуем конечный период
        var endPeriod = dateTo.Date;
        if (groupPeriod == GroupPeriod.Month)
        {
            endPeriod = new DateTime(endPeriod.Year, endPeriod.Month, 1);
        }
        else if (groupPeriod == GroupPeriod.Year)
        {
            endPeriod = new DateTime(endPeriod.Year, 1, 1);
        }

        while (currentPeriod <= endPeriod)
        {
            result.Add(new ZasNetEarningByPeriodDto
            {
                Period = currentPeriod,
                GroupPeriod = groupPeriod,
                TotalEarning = earningsDict.GetValueOrDefault(currentPeriod, 0)
            });

            currentPeriod = groupPeriod switch
            {
                GroupPeriod.Day => currentPeriod.AddDays(1),
                GroupPeriod.Month => currentPeriod.AddMonths(1),
                GroupPeriod.Year => currentPeriod.AddYears(1),
                _ => currentPeriod.AddDays(1)
            };
        }

        return result;
    }
}

