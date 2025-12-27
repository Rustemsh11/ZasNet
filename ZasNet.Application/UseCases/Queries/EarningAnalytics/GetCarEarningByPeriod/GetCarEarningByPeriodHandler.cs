using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.EarningAnalytics.GetCarEarningByPeriod;

/// <summary>
/// Обработчик запроса на получение прибыли машин по периодам
/// </summary>
public class GetCarEarningByPeriodHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetCarEarningByPeriodRequest, List<CarEarningByPeriodDto>>
{
    public async Task<List<CarEarningByPeriodDto>> Handle(
        GetCarEarningByPeriodRequest request,
        CancellationToken cancellationToken)
    {
        // Базовый запрос для получения данных о машинах и заявках
        var query = repositoryManager.OrderCarRepository
            .FindAll(false)
            .Include(osc => osc.Car)
                .ThenInclude(osc => osc.CarModel)
            .Include(osc => osc.OrderService)
                .ThenInclude(os => os.Order)
            .Where(osc => osc.OrderService.Order.DateStart >= request.DateFrom
                       && osc.OrderService.Order.DateStart <= request.DateTo);

        if (request.CarIds != null && request.CarIds.Any())
        {
            query = query.Where(osc => request.CarIds.Contains(osc.CarId));
        }

        var orderServiceCars = await query.ToListAsync(cancellationToken);

        // Создаем словарь с количеством машин для каждой услуги
        var carsPerService = orderServiceCars
            .GroupBy(osc => osc.OrderServiceId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Группировка в зависимости от выбранного типа периода
        return request.GroupPeriod switch
        {
            GroupPeriod.Day => GetDailyEarnings(orderServiceCars, carsPerService),
            GroupPeriod.Month => GetMonthlyEarnings(orderServiceCars, carsPerService),
            GroupPeriod.Year => GetYearlyEarnings(orderServiceCars, carsPerService),
            _ => throw new ArgumentException($"Неизвестный тип периода: {request.GroupPeriod}")
        };
    }

    /// <summary>
    /// Группировка по дням
    /// </summary>
    private List<CarEarningByPeriodDto> GetDailyEarnings(List<OrderServiceCar> orderServiceCars, Dictionary<int, int> carsPerService)
    {
        // Группируем по машинам и дням, избегая двойного подсчета услуг
        var processedServices = new HashSet<int>();
        var grouped = orderServiceCars
            .GroupBy(osc => new
            {
                Date = osc.OrderService.Order.DateStart.Date,
                CarId = osc.CarId,
                CarNumber = osc.Car.Number,
                CarModel = osc.Car.CarModel.Name
            })
            .Select(g =>
            {
                var totalEarning = 0m;
                processedServices.Clear();

                foreach (var osc in g)
                {
                    if (processedServices.Contains(osc.OrderServiceId))
                        continue;

                    processedServices.Add(osc.OrderServiceId);
                    // Делим PriceTotal на количество машин, выполняющих эту услугу
                    var carsCount = carsPerService[osc.OrderServiceId];
                    totalEarning += osc.OrderService.PriceTotal / carsCount;
                }

                return new CarEarningByPeriodDto
                {
                    Period = g.Key.Date,
                    GroupPeriod = GroupPeriod.Day,
                    CarId = g.Key.CarId,
                    CarNumber = g.Key.CarNumber,
                    CarModel = g.Key.CarModel,
                    TotalEarning = totalEarning
                };
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.CarNumber)
            .ToList();

        return grouped;
    }

    /// <summary>
    /// Группировка по месяцам
    /// </summary>
    private List<CarEarningByPeriodDto> GetMonthlyEarnings(List<OrderServiceCar> orderServiceCars, Dictionary<int, int> carsPerService)
    {
        var processedServices = new HashSet<int>();
        var grouped = orderServiceCars
            .GroupBy(osc => new
            {
                Year = osc.OrderService.Order.DateStart.Year,
                Month = osc.OrderService.Order.DateStart.Month,
                FirstDayOfMonth = new DateTime(osc.OrderService.Order.DateStart.Year, osc.OrderService.Order.DateStart.Month, 1),
                CarId = osc.CarId,
                CarNumber = osc.Car.Number,
                CarModel = osc.Car.CarModel.Name
            })
            .Select(g =>
            {
                var totalEarning = 0m;
                processedServices.Clear();

                foreach (var osc in g)
                {
                    if (processedServices.Contains(osc.OrderServiceId))
                        continue;

                    processedServices.Add(osc.OrderServiceId);
                    // Делим PriceTotal на количество машин, выполняющих эту услугу
                    var carsCount = carsPerService[osc.OrderServiceId];
                    totalEarning += osc.OrderService.PriceTotal / carsCount;
                }

                return new CarEarningByPeriodDto
                {
                    Period = g.Key.FirstDayOfMonth,
                    GroupPeriod = GroupPeriod.Month,
                    CarId = g.Key.CarId,
                    CarNumber = g.Key.CarNumber,
                    CarModel = g.Key.CarModel,
                    TotalEarning = totalEarning
                };
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.CarNumber)
            .ToList();

        return grouped;
    }

    /// <summary>
    /// Группировка по годам
    /// </summary>
    private List<CarEarningByPeriodDto> GetYearlyEarnings(List<OrderServiceCar> orderServiceCars, Dictionary<int, int> carsPerService)
    {
        var processedServices = new HashSet<int>();
        var grouped = orderServiceCars
            .GroupBy(osc => new
            {
                Year = osc.OrderService.Order.DateStart.Year,
                FirstDayOfYear = new DateTime(osc.OrderService.Order.DateStart.Year, 1, 1),
                CarId = osc.CarId,
                CarNumber = osc.Car.Number,
                CarModel = osc.Car.CarModel.Name
            })
            .Select(g =>
            {
                var totalEarning = 0m;
                processedServices.Clear();

                foreach (var osc in g)
                {
                    if (processedServices.Contains(osc.OrderServiceId))
                        continue;

                    processedServices.Add(osc.OrderServiceId);
                    // Делим PriceTotal на количество машин, выполняющих эту услугу
                    var carsCount = carsPerService[osc.OrderServiceId];
                    totalEarning += osc.OrderService.PriceTotal / carsCount;
                }

                return new CarEarningByPeriodDto
                {
                    Period = g.Key.FirstDayOfYear,
                    GroupPeriod = GroupPeriod.Year,
                    CarId = g.Key.CarId,
                    CarNumber = g.Key.CarNumber,
                    CarModel = g.Key.CarModel,
                    TotalEarning = totalEarning
                };
            })
            .OrderBy(r => r.Period)
            .ThenBy(r => r.CarNumber)
            .ToList();

        return grouped;
    }
}

