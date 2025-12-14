namespace ZasNet.Domain.Entities;

public class EmployeeEarinig : BaseItem
{
    public int OrderServiceId {  get; set; }

    public decimal ServiceEmployeePrecent { get; set; }

    public string? PrecentEmployeeDescription { get; set; }
    
    public decimal EmployeeEarning { get; set; }

    public OrderService OrderService { get; set; }

    public static EmployeeEarinig CreateEmployeeEarning(CreateEmployeeEarningDto orderService)
    {
        var (percent, description) = GetPrecent(orderService);
        return new EmployeeEarinig()
        {
            ServiceEmployeePrecent = percent,
            PrecentEmployeeDescription = description,
            EmployeeEarning = GetEarning(orderService, percent),
        };
    }

    public class CreateEmployeeEarningDto
    {
        public decimal StandartPrecentForEmployee { get; set; }
        public decimal PrecentForMultipleEmployeers { get; set; }
        public decimal PrecentLaterOrderForEmployee { get; set; }
        public decimal PrecentLaterOrderForMultipleEmployeers { get; set; }

        public int OrderServiceEmployeesCount { get; set; }

        public DateTime OrderStartDateTime { get; set; }

        public decimal TotalPrice { get; set; }
    }

    private static (decimal Precent, string Description) GetPrecent(CreateEmployeeEarningDto orderService)
    {
        var precent = orderService.StandartPrecentForEmployee;
        var descriptionBuilder = $"Стандартный процент за выполнение услуги 1 сотрудником";
        
        if (orderService.OrderServiceEmployeesCount > 1)
        {
            if (orderService.OrderStartDateTime.TimeOfDay.Hours > TimeSpan.FromHours(18).Hours)
            {
                precent = orderService.PrecentLaterOrderForMultipleEmployeers;
                descriptionBuilder = $"Процент за выполнение услуги после 18:00 несколькими сотрудниками";
            }
            else
            {
                precent = orderService.PrecentForMultipleEmployeers;
                descriptionBuilder = $"Процент за выполнение услуги нескольким сотрудником";
            }
        }
        else
        {
            if (orderService.OrderStartDateTime.TimeOfDay.Hours > TimeSpan.FromHours(18).Hours)
            {
                precent = orderService.PrecentLaterOrderForEmployee;
                descriptionBuilder = $"Процент за выполнение услуги после 18:00 1 сотрудником";
            }
        }

        return (precent, descriptionBuilder);
    }

    private static decimal GetEarning(CreateEmployeeEarningDto orderService, decimal precent)
    {
        var percentForEmployee = Math.Ceiling(precent / orderService.OrderServiceEmployeesCount);

        return orderService.TotalPrice * percentForEmployee/100.0M;
    }
}
