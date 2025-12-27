using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Filters;
using ZasNet.Application.UseCases.Queries.EarningAnalytics.GetEarningAnalytics.Visitors;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetCarEarningAnalytics;

/// <summary>
/// Обработчик запроса на получение аналитики заработков по машинам
/// </summary>
public class GetCarEarningAnalyticsHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetCarEarningAnalyticsRequest, List<CarEarningAnalyticsDto>>
{
    public async Task<List<CarEarningAnalyticsDto>> Handle(
        GetCarEarningAnalyticsRequest request,
        CancellationToken cancellationToken)
    {
        // Начинаем с базового запроса с необходимыми включениями
        // Убираем циклический Include OrderServiceEmployees, чтобы избежать цикла в no-tracking запросах
        IQueryable<ZasNet.Domain.Entities.OrderServiceCar> query = repositoryManager.OrderCarRepository.FindAll(false)
            .Include(osc => osc.Car)
                .ThenInclude(c => c.CarModel)
            .Include(osc => osc.OrderService)
                .ThenInclude(os => os.Order);

        // Создаем список фильтров на основе запроса
        var filters = BuildFilters(request);

        // Применяем все фильтры через паттерн Visitor
        var visitor = new CarAnalyticsFilterVisitor();
        foreach (var filter in filters)
        {
            query = filter.Accept(visitor, query);
        }

        var carData = await query.ToListAsync(cancellationToken);

        // Создаем словарь с количеством машин для каждой услуги
        var carsPerService = carData
            .GroupBy(osc => osc.OrderServiceId)
            .ToDictionary(g => g.Key, g => g.Count());

        var analytics = carData
            .GroupBy(osc => new { osc.CarId, osc.Car.Number, CarModelName = osc.Car.CarModel != null ? osc.Car.CarModel.Name : null })
            .Select(g =>
            {
                var totalEarnings = 0m;
                var processedServices = new HashSet<int>();

                foreach (var osc in g)
                {
                    // Избегаем двойного подсчета для одной услуги
                    if (processedServices.Contains(osc.OrderServiceId))
                        continue;

                    processedServices.Add(osc.OrderServiceId);
                    // Делим PriceTotal на количество машин, выполняющих эту услугу
                    var carsCount = carsPerService[osc.OrderServiceId];
                    totalEarnings += osc.OrderService.PriceTotal / carsCount;
                }

                return new CarEarningAnalyticsDto
                {
                    CarId = g.Key.CarId,
                    CarNumber = g.Key.Number,
                    CarModel = g.Key.CarModelName,
                    TotalEarnings = totalEarnings,
                    OrdersCount = g.Select(osc => osc.OrderService.OrderId).Distinct().Count(),
                };
            })
            .ToList();

        return analytics;
    }

    /// <summary>
    /// Создает список фильтров для аналитики по машинам на основе параметров запроса
    /// </summary>
    private List<ICarAnalyticsFilter> BuildFilters(GetCarEarningAnalyticsRequest request)
    {
        var filters = new List<ICarAnalyticsFilter>();

        // Обязательный фильтр по диапазону дат
        filters.Add(new DateRangeFilter
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo
        });

        // Фильтр по машинам
        if (request.CarIds != null && request.CarIds.Any())
        {
            filters.Add(new CarFilter
            {
                CarIds = request.CarIds
            });
        }

        return filters;
    }
}

