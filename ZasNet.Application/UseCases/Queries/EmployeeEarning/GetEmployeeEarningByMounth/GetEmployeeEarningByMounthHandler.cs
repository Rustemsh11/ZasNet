using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Обработчик запроса на получение заработков сотрудников за месяц с фильтрацией
/// </summary>
public class GetEmployeeEarningByMounthHandler(IRepositoryManager repositoryManager)
    : IRequestHandler<GetEmployeeEarningByMounthRequest, List<GetEmployeeEarningByMounthResponse>>
{
    public async Task<List<GetEmployeeEarningByMounthResponse>> Handle(
        GetEmployeeEarningByMounthRequest request,
        CancellationToken cancellationToken)
    {
        // Начинаем с базового запроса с необходимыми включениями
        IQueryable<EmployeeEarinig> query = repositoryManager.EmployeeEarningRepository.FindAll(false)
            .Include(ee => ee.OrderService)
                .ThenInclude(os => os.Order)
            .Include(ee => ee.OrderService)
                .ThenInclude(os => os.Service)
            .Include(ee => ee.OrderService)
                .ThenInclude(os => os.OrderServiceEmployees)
                    .ThenInclude(ose => ose.Employee);

        // Создаем список фильтров на основе запроса
        var filters = BuildFilters(request);

        // Применяем все фильтры через паттерн Visitor
        var visitor = new EmployeeEarningFilterVisitor();
        foreach (var filter in filters)
        {
            query = filter.Accept(visitor, query);
        }

        // Выполняем запрос
        var earnings = await query
            .OrderByDescending(ee => ee.OrderService.Order.DateStart)
            .ToListAsync(cancellationToken);

        // Маппим результаты - для каждого сотрудника создаем отдельную запись
        var result = new List<GetEmployeeEarningByMounthResponse>();
        
        foreach (var ee in earnings)
        {
            // Получаем список сотрудников для фильтрации
            var employeesToInclude = ee.OrderService.OrderServiceEmployees.AsEnumerable();
            
            // Если есть фильтр по сотрудникам, применяем его
            if (request.EmployeeIds != null && request.EmployeeIds.Any())
            {
                employeesToInclude = employeesToInclude.Where(ose => request.EmployeeIds.Contains(ose.EmployeeId));
            }
            
            // Для каждого сотрудника, назначенного на OrderService, создаем отдельную запись
            foreach (var employee in employeesToInclude)
            {
                result.Add(new GetEmployeeEarningByMounthResponse
                {
                    OrderId = ee.OrderService.OrderId,
                    Client = ee.OrderService.Order.Client,
                    OrderDateStart = ee.OrderService.Order.DateStart,
                    OrderDateEnd = ee.OrderService.Order.DateEnd,
                    ServiceName = ee.OrderService.Service.Name,
                    Employee = new CommonDtos.EmployeeDto
                    {
                        Id = employee.Employee.Id,
                        Name = employee.Employee.Name
                    },
                    EmployeeEarningDto = new CommonDtos.EmployeeEarningDto()
                    {
                        EmployeeEarningId = ee.Id,
                        ServiceEmployeePrecent = ee.ServiceEmployeePrecent,
                        PrecentEmployeeDescription = ee.PrecentEmployeeDescription,
                        EmployeeEarning = ee.EmployeeEarning,
                    },
                    ServiceTotalPrice = ee.OrderService.PriceTotal,
                    TotalVolume = ee.OrderService.TotalVolume
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Создает список фильтров на основе параметров запроса
    /// </summary>
    private List<IEmployeeEarningFilter> BuildFilters(GetEmployeeEarningByMounthRequest request)
    {
        var filters = new List<IEmployeeEarningFilter>();

        // Обязательный фильтр по месяцу и году
        filters.Add(new MonthYearFilter
        {
            Year = request.Year,
            Month = request.Month
        });

        // Фильтр по сотрудникам
        if (request.EmployeeIds != null && request.EmployeeIds.Any())
        {
            filters.Add(new EmployeeFilter
            {
                EmployeeIds = request.EmployeeIds
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

        // Фильтр по услугам
        if (request.ServiceIds != null && request.ServiceIds.Any())
        {
            filters.Add(new ServiceFilter
            {
                ServiceIds = request.ServiceIds
            });
        }

        return filters;
    }
}

