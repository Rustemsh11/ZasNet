using MediatR;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Запрос на получение заявок с фильтрацией
/// </summary>
public class GetOrdersByFilterRequest : IRequest<List<GetOrdersByFilterResponse>>
{
    /// <summary>
    /// Дата начала диапазона
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Дата окончания диапазона
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Список статусов для фильтрации
    /// </summary>
    public List<OrderStatus>? Statuses { get; set; }

    /// <summary>
    /// Поисковый запрос по имени клиента
    /// </summary>
    public string? ClientSearchTerm { get; set; }

    /// <summary>
    /// Список типов оплаты для фильтрации
    /// </summary>
    public List<PaymentType>? PaymentTypes { get; set; }

    /// <summary>
    /// Список ID услуг для фильтрации
    /// </summary>
    public List<int>? ServiceIds { get; set; }

    /// <summary>
    /// Список ID сотрудников, создавших заявку
    /// </summary>
    public List<int>? CreatedEmployeeIds { get; set; }
}

