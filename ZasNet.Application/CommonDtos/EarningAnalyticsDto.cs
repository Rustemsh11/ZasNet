namespace ZasNet.Application.CommonDtos;

/// <summary>
/// Аналитика заработков по машинам
/// </summary>
public class CarEarningAnalyticsDto
{
    /// <summary>
    /// ID машины
    /// </summary>
    public int CarId { get; set; }

    /// <summary>
    /// Номер машины
    /// </summary>
    public string CarNumber { get; set; } = string.Empty;

    /// <summary>
    /// Модель машины
    /// </summary>
    public string? CarModel { get; set; }

    /// <summary>
    /// Общий заработок водителей, работавших на этой машине
    /// </summary>
    public decimal TotalEarnings { get; set; }

    /// <summary>
    /// Количество заявок
    /// </summary>
    public int OrdersCount { get; set; }
}

/// <summary>
/// Аналитика заработков по услугам
/// </summary>
public class ServiceEarningAnalyticsDto
{
    /// <summary>
    /// ID услуги
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Название услуги
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Общий заработок сотрудников за эту услугу
    /// </summary>
    public decimal TotalEmployeesEarnings { get; set; }

    /// <summary>
    /// Количество заявок
    /// </summary>
    public int OrdersCount { get; set; }

    /// <summary>
    /// Количество выполненных услуг
    /// </summary>
    public int ServicesCount { get; set; }

    /// <summary>
    /// Общая стоимость услуг
    /// </summary>
    public decimal TotalZasNetEarning { get; set; }
}

/// <summary>
/// Аналитика заработков ZasNet по водителям (сотрудникам)
/// </summary>
public class DriverEarningAnalyticsDto
{
    /// <summary>
    /// ID водителя
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Имя водителя
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Общий заработок водителя
    /// </summary>
    public decimal TotalZasNetEarningByEmployee { get; set; }

    /// <summary>
    /// Количество заявок
    /// </summary>
    public int OrdersCount { get; set; }
}

/// <summary>
/// Аналитика заработков по диспетчерам
/// </summary>
public class DispatcherEarningAnalyticsDto
{
    /// <summary>
    /// ID диспетчера
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Имя диспетчера
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Общий заработок диспетчера
    /// </summary>
    public decimal TotalDispetcherEarnings { get; set; }

    public decimal TotalZasNetEarningFromDispetcher { get; set; }

    /// <summary>
    /// Количество заявок
    /// </summary>
    public int OrdersCount { get; set; }

}


