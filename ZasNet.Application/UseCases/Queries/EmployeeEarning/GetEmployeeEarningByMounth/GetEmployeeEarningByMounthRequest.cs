using MediatR;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Запрос на получение заработков сотрудников за месяц с фильтрацией
/// </summary>
public class GetEmployeeEarningByMounthRequest : IRequest<List<GetEmployeeEarningByMounthResponse>>
{
    /// <summary>
    /// Год (обязательный)
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Месяц (обязательный, 1-12)
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Список ID сотрудников для фильтрации
    /// </summary>
    public List<int>? EmployeeIds { get; set; }

    /// <summary>
    /// Поисковый запрос по имени клиента
    /// </summary>
    public string? ClientSearchTerm { get; set; }

    /// <summary>
    /// Дата начала диапазона для дополнительной фильтрации по датам заявки
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Дата окончания диапазона для дополнительной фильтрации по датам заявки
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Список ID услуг для фильтрации
    /// </summary>
    public List<int>? ServiceIds { get; set; }
}

