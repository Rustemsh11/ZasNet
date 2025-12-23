using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetServiceEarningByPeriod;

/// <summary>
/// Обработчик запроса на получение прибыли услуг по периодам
/// </summary>
public class GetServiceEarningByPeriodHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetServiceEarningByPeriodRequest, List<ServiceEarningByPeriodDto>>
{
    public async Task<List<ServiceEarningByPeriodDto>> Handle(
        GetServiceEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        List<OrderService> orderServices;

        if (request.ServiceIds != null && request.ServiceIds.Any())
        {
            // Базовый запрос для получения данных об услугах и заявках
            orderServices = await repositoryManager.OrderServiceRepository
                .FindAll(false)
                .Include(os => os.Service)
                .Include(os => os.Order)
                .Where(os => os.Order.DateStart >= request.DateFrom
                          && os.Order.DateStart <= request.DateTo
                          && request.ServiceIds.Contains(os.ServiceId))
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Базовый запрос для получения данных об услугах и заявках
            orderServices = await repositoryManager.OrderServiceRepository
                .FindAll(false)
                .Include(os => os.Service)
                .Include(os => os.Order)
                .Where(os => os.Order.DateStart >= request.DateFrom
                          && os.Order.DateStart <= request.DateTo)
                .ToListAsync(cancellationToken);
        }

        // Группировка в зависимости от выбранного типа периода
        return request.GroupPeriod switch
        {
            GroupPeriod.Day => GetDailyEarnings(orderServices),
            GroupPeriod.Month => GetMonthlyEarnings(orderServices),
            GroupPeriod.Year => GetYearlyEarnings(orderServices),
            _ => throw new ArgumentException($"Неизвестный тип периода: {request.GroupPeriod}")
        };
    }

    /// <summary>
    /// Группировка по дням
    /// </summary>
    private List<ServiceEarningByPeriodDto> GetDailyEarnings(List<ZasNet.Domain.Entities.OrderService> orderServices)
    {
        return orderServices
            .GroupBy(os => new
            {
                Date = os.Order.DateStart.Date,
                ServiceId = os.ServiceId,
                ServiceName = os.Service.Name
            })
            .Select(g => new ServiceEarningByPeriodDto
            {
                Period = g.Key.Date,
                GroupPeriod = GroupPeriod.Day,
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.ServiceName,
                TotalEarning = g.Sum(os => os.PriceTotal)
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.ServiceName)
            .ToList();
    }

    /// <summary>
    /// Группировка по месяцам
    /// </summary>
    private List<ServiceEarningByPeriodDto> GetMonthlyEarnings(List<ZasNet.Domain.Entities.OrderService> orderServices)
    {
        return orderServices
            .GroupBy(os => new
            {
                Year = os.Order.DateStart.Year,
                Month = os.Order.DateStart.Month,
                FirstDayOfMonth = new DateTime(os.Order.DateStart.Year, os.Order.DateStart.Month, 1),
                ServiceId = os.ServiceId,
                ServiceName = os.Service.Name
            })
            .Select(g => new ServiceEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfMonth,
                GroupPeriod = GroupPeriod.Month,
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.ServiceName,
                TotalEarning = g.Sum(os => os.PriceTotal)
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.ServiceName)
            .ToList();
    }

    /// <summary>
    /// Группировка по годам
    /// </summary>
    private List<ServiceEarningByPeriodDto> GetYearlyEarnings(List<ZasNet.Domain.Entities.OrderService> orderServices)
    {
        return orderServices
            .GroupBy(os => new
            {
                Year = os.Order.DateStart.Year,
                FirstDayOfYear = new DateTime(os.Order.DateStart.Year, 1, 1),
                ServiceId = os.ServiceId,
                ServiceName = os.Service.Name
            })
            .Select(g => new ServiceEarningByPeriodDto
            {
                Period = g.Key.FirstDayOfYear,
                GroupPeriod = GroupPeriod.Year,
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.ServiceName,
                TotalEarning = g.Sum(os => os.PriceTotal)
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.ServiceName)
            .ToList();
    }
}

