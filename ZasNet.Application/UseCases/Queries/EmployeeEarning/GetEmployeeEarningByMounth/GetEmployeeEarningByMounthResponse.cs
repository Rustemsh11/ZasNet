namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;

/// <summary>
/// Ответ с данными о заработке сотрудника
/// </summary>
public class GetEmployeeEarningByMounthResponse
{
    /// <summary>
    /// ID записи о заработке
    /// </summary>
    public int Id { get; set; }

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
    public EmployeeInfo Employee { get; set; } = new();

    /// <summary>
    /// Процент для сотрудника
    /// </summary>
    public decimal ServiceEmployeePrecent { get; set; }

    /// <summary>
    /// Описание процента
    /// </summary>
    public string? PrecentEmployeeDescription { get; set; }

    /// <summary>
    /// Заработок сотрудника
    /// </summary>
    public decimal EmployeeEarning { get; set; }

    /// <summary>
    /// Общая стоимость услуги
    /// </summary>
    public decimal ServiceTotalPrice { get; set; }

    /// <summary>
    /// Объем работ
    /// </summary>
    public double TotalVolume { get; set; }
}

/// <summary>
/// Информация о сотруднике
/// </summary>
public class EmployeeInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

