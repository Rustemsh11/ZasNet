using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetDailyEarnings;

/// <summary>
/// Обработчик запроса на получение заработка по дням
/// </summary>
public class GetDailyEarningsHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetDailyEarningsRequest, Dictionary<DateTime, decimal>>
{
    public async Task<Dictionary<DateTime, decimal>> Handle(
        GetDailyEarningsRequest request,
        CancellationToken cancellationToken)
    {
        // Получаем заработок заявок по дням (OrderPriceAmount)
        var orderEarnings = await repositoryManager.OrderRepository
            .FindAll(false)
            .Where(o => o.DateStart >= request.DateFrom
                     && o.DateStart <= request.DateTo)
            .GroupBy(o => o.DateStart.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalEarning = g.Sum(o => o.OrderPriceAmount)
            })
            .ToListAsync(cancellationToken);

        // Преобразуем в словарь
        var dailyEarnings = orderEarnings.ToDictionary(oe => oe.Date, oe => oe.TotalEarning);

        // Заполняем дни без заработка нулями для полного диапазона
        var allDays = new Dictionary<DateTime, decimal>();
        var currentDate = request.DateFrom.Date;
        var endDate = request.DateTo.Date;

        while (currentDate <= endDate)
        {
            allDays[currentDate] = dailyEarnings.GetValueOrDefault(currentDate, 0);
            currentDate = currentDate.AddDays(1);
        }

        return allDays;
    }
}

