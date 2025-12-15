using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Обработчик запроса на получение заработков диспетчеров за месяц с фильтрацией
/// </summary>
public class GetDispetcherEarningByMounthHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetDispetcherEarningByMounthRequest, List<GetDispetcherEarningByMounthResponse>>
{
    public async Task<List<GetDispetcherEarningByMounthResponse>> Handle(
        GetDispetcherEarningByMounthRequest request,
        CancellationToken cancellationToken)
    {
        // Начинаем с базового запроса с необходимыми включениями
        IQueryable<DispetcherEarning> query = repositoryManager.DispetcherEarningRepository.FindAll(false)
            .Include(de => de.Order)
                .ThenInclude(o => o.CreatedEmployee)
            .Include(de => de.Order)
                .ThenInclude(o => o.OrderServices);

        // Создаем список фильтров на основе запроса
        var filters = BuildFilters(request);

        // Применяем все фильтры через паттерн Visitor
        var visitor = new DispetcherEarningFilterVisitor();
        foreach (var filter in filters)
        {
            query = filter.Accept(visitor, query);
        }

        // Выполняем запрос
        var earnings = await query
            .OrderByDescending(de => de.Order.DateStart)
            .ToListAsync(cancellationToken);

        // Маппим результаты
        var result = earnings.Select(de => new GetDispetcherEarningByMounthResponse
        {
            Id = de.Id,
            OrderId = de.OrderId,
            Client = de.Order.Client,
            OrderDateStart = de.Order.DateStart,
            OrderDateEnd = de.Order.DateEnd,
            Dispetcher = new CommonDtos.EmployeeDto
            {
                Id = de.Order.CreatedEmployee.Id,
                Name = de.Order.CreatedEmployee.Name
            },
            DispetcherPrecent = de.ServiceEmployeePrecent,
            PrecentDispetcherDescription = de.PrecentEmployeeDescription,
            DispetcherEarning = de.EmployeeEarning,
            OrderTotalPrice = de.Order.OrderPriceAmount
        }).ToList();

        return result;
    }

    /// <summary>
    /// Создает список фильтров на основе параметров запроса
    /// </summary>
    private List<IDispetcherEarningFilter> BuildFilters(GetDispetcherEarningByMounthRequest request)
    {
        var filters = new List<IDispetcherEarningFilter>();

        // Обязательный фильтр по месяцу и году
        filters.Add(new MonthYearFilter
        {
            Year = request.Year,
            Month = request.Month
        });

        // Фильтр по диспетчерам
        if (request.DispetcherIds != null && request.DispetcherIds.Any())
        {
            filters.Add(new DispetcherFilter
            {
                DispetcherIds = request.DispetcherIds
            });
        }

        // Фильтр по клиенту
        if (!string.IsNullOrWhiteSpace(request.ClientSearchTerm))
        {
            filters.Add(new ClientFilter
            {
                ClientSearchTerm = request.ClientSearchTerm
            });
        }

        // Дополнительный фильтр по датам заявки
        if (request.DateFrom.HasValue || request.DateTo.HasValue)
        {
            filters.Add(new OrderDateFilter
            {
                DateFrom = request.DateFrom,
                DateTo = request.DateTo
            });
        }

        return filters;
    }
}

