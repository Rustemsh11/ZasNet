using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Ответ с данными о заработке сотрудника
/// </summary>
public class GetEmployeeEarningByMounthResponse
{
    /// <summary>
    /// ID заявки
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Клиент заявки
    /// </summary>
    public string Client { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала заявки
    /// </summary>
    public DateTime OrderDateStart { get; set; }

    /// <summary>
    /// Дата окончания заявки
    /// </summary>
    public DateTime OrderDateEnd { get; set; }

    /// <summary>
    /// Название услуги
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Сотрудник
    /// </summary>
    public EmployeeDto Employee { get; set; } = new();

    public EmployeeEarningDto EmployeeEarningDto { get; set; }

    /// <summary>
    /// Общая стоимость услуги
    /// </summary>
    public decimal ServiceTotalPrice { get; set; }

    /// <summary>
    /// Объем работ
    /// </summary>
    public double TotalVolume { get; set; }
}

