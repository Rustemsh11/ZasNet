using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDispatcherEarningByPeriod;

/// <summary>
/// Обработчик запроса на получение прибыли диспетчеров по периодам
/// </summary>
public class GetDispatcherEarningByPeriodHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetDispatcherEarningByPeriodRequest, List<DispatcherEarningByPeriodDto>>
{
    public async Task<List<DispatcherEarningByPeriodDto>> Handle(
        GetDispatcherEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        // Базовый запрос для получения данных о диспетчерах и заявках
        var query = repositoryManager.DispetcherEarningRepository
            .FindAll(false)
            .Include(de => de.Order)
                .ThenInclude(o => o.CreatedEmployee)
            .Where(de => de.Order.DateStart >= request.DateFrom
                      && de.Order.DateStart <= request.DateTo);

        if (request.DispatcherIds != null && request.DispatcherIds.Any())
        {
            query = query.Where(de => request.DispatcherIds.Contains(de.Order.CreatedEmployeeId));
        }

        var dispatcherEarnings = await query.ToListAsync(cancellationToken);

        // Группировка в зависимости от выбранного типа периода
        return request.GroupPeriod switch
        {
            GroupPeriod.Day => GetDailyEarnings(dispatcherEarnings),
            GroupPeriod.Month => GetMonthlyEarnings(dispatcherEarnings),
            GroupPeriod.Year => GetYearlyEarnings(dispatcherEarnings),
            _ => throw new ArgumentException($"Неизвестный тип периода: {request.GroupPeriod}")
        };
    }

    /// <summary>
    /// Группировка по дням
    /// </summary>
    private List<DispatcherEarningByPeriodDto> GetDailyEarnings(List<ZasNet.Domain.Entities.DispetcherEarning> dispatcherEarnings)
    {
        return dispatcherEarnings
            .GroupBy(de => new
            {
                Date = de.Order.DateStart.Date,
                DispatcherId = de.Order.CreatedEmployeeId,
                DispatcherName = de.Order.CreatedEmployee.Name
            })
            .Select(g => new DispatcherEarningByPeriodDto
            {
                Period = g.Key.Date,
                GroupPeriod = GroupPeriod.Day,
                DispatcherId = g.Key.DispatcherId,
                DispatcherName = g.Key.DispatcherName,
                TotalEarning = g.Sum(de => de.Order.OrderPriceAmount)
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.DispatcherName)
            .ToList();
    }

    /// <summary>
    /// Группировка по месяцам
    /// </summary>
    private List<DispatcherEarningByPeriodDto> GetMonthlyEarnings(List<ZasNet.Domain.Entities.DispetcherEarning> dispatcherEarnings)
    {
        return dispatcherEarnings
            .GroupBy(de => new
            {
                Year = de.Order.DateStart.Year,
                Month = de.Order.DateStart.Month,
                FirstDayOfMonth = new DateTime(de.Order.DateStart.Year, de.Order.DateStart.Month, 1),
                DispatcherId = de.Order.CreatedEmployeeId,
                DispatcherName = de.Order.CreatedEmployee.Name
            })
            .Select(g => new DispatcherEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfMonth,
                GroupPeriod = GroupPeriod.Month,
                DispatcherId = g.Key.DispatcherId,
                DispatcherName = g.Key.DispatcherName,
                TotalEarning = g.Sum(de => de.Order.OrderPriceAmount)
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.DispatcherName)
            .ToList();
    }

    /// <summary>
    /// Группировка по годам
    /// </summary>
    private List<DispatcherEarningByPeriodDto> GetYearlyEarnings(List<ZasNet.Domain.Entities.DispetcherEarning> dispatcherEarnings)
    {
        return dispatcherEarnings
            .GroupBy(de => new
            {
                Year = de.Order.DateStart.Year,
                FirstDayOfYear = new DateTime(de.Order.DateStart.Year, 1, 1),
                DispatcherId = de.Order.CreatedEmployeeId,
                DispatcherName = de.Order.CreatedEmployee.Name
            })
            .Select(g => new DispatcherEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfYear,
                GroupPeriod = GroupPeriod.Year,
                DispatcherId = g.Key.DispatcherId,
                DispatcherName = g.Key.DispatcherName,
                TotalEarning = g.Sum(de => de.Order.OrderPriceAmount)
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.DispatcherName)
            .ToList();
    }
}

