using MediatR;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

/// <summary>
/// Запрос на получение заработков диспетчеров за месяц с фильтрацией
/// </summary>
public class GetDispetcherEarningByMounthRequest : IRequest<List<GetDispetcherEarningByMounthResponse>>
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
    /// Список ID диспетчеров для фильтрации
    /// </summary>
    public List<int>? DispetcherIds { get; set; }

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
}

