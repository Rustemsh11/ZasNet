using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Обработчик запроса на получение заявок с фильтрацией
/// </summary>
public class GetOrdersByFilterHandler(
    IRepositoryManager repositoryManager,
    IMapper mapper)
    : IRequestHandler<GetOrdersByFilterRequest, List<GetOrdersByFilterResponse>>
{
    public async Task<List<GetOrdersByFilterResponse>> Handle(
        GetOrdersByFilterRequest request,
        CancellationToken cancellationToken)
    {
        // Начинаем с базового запроса
        IQueryable<Order> query = repositoryManager.OrderRepository.FindAll(false)
            .Include(o => o.CreatedEmployee)
            .Include(o => o.OrderServices)
                .ThenInclude(os => os.Service)
            .Include(o => o.OrderServices)
                .ThenInclude(os => os.OrderServiceCars)
                    .ThenInclude(osc => osc.Car);

        // Создаем список фильтров на основе запроса
        var filters = BuildFilters(request);

        // Применяем все фильтры через паттерн Visitor
        var visitor = new OrderFilterVisitor();
        foreach (var filter in filters)
        {
            query = filter.Accept(visitor, query);
        }

        // Выполняем запрос
        var orders = await query
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);

        // Маппим результаты
        return mapper.Map<List<GetOrdersByFilterResponse>>(orders);
    }

    /// <summary>
    /// Создает список фильтров на основе параметров запроса
    /// </summary>
    private List<IOrderFilter> BuildFilters(GetOrdersByFilterRequest request)
    {
        var filters = new List<IOrderFilter>();

        // Фильтр по датам
        if (request.DateFrom.HasValue || request.DateTo.HasValue)
        {
            filters.Add(new DateRangeFilter
            {
                DateFrom = request.DateFrom,
                DateTo = request.DateTo
            });
        }

        // Фильтр по статусам
        if (request.Statuses != null && request.Statuses.Any())
        {
            filters.Add(new StatusFilter
            {
                Statuses = request.Statuses
            });
        }

        // Фильтр поиска по клиенту
        if (!string.IsNullOrWhiteSpace(request.ClientSearchTerm))
        {
            filters.Add(new ClientSearchFilter
            {
                SearchTerm = request.ClientSearchTerm
            });
        }

        // Фильтр по типам оплаты
        if (request.PaymentTypes != null && request.PaymentTypes.Any())
        {
            filters.Add(new PaymentTypeFilter
            {
                PaymentTypes = request.PaymentTypes
            });
        }

        // Фильтр по услугам
        if (request.ServiceIds != null && request.ServiceIds.Any())
        {
            filters.Add(new ServiceFilter
            {
                ServiceIds = request.ServiceIds
            });
        }

        // Фильтр по создавшим сотрудникам
        if (request.CreatedEmployeeIds != null && request.CreatedEmployeeIds.Any())
        {
            filters.Add(new CreatedEmployeeFilter
            {
                EmployeeIds = request.CreatedEmployeeIds
            });
        }

        return filters;
    }
}

