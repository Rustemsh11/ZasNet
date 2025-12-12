using System.Text;

namespace ZasNet.Domain.Entities;

public class EmployeeEarinig : BaseItem
{
    public int OrderServiceId {  get; set; }

    public decimal ServiceEmployeePrecent { get; set; }

    public string? PrecentEmployeeDescription { get; set; }
    
    public decimal EmployeeEarning { get; set; }

    public OrderService OrderService { get; set; }

    public static EmployeeEarinig CreateEmployeeEarning(OrderService orderService)
    {
        var (percent, description) = GetPrecent(orderService);
        return new EmployeeEarinig()
        {
            ServiceEmployeePrecent = percent,
            PrecentEmployeeDescription = description,
            EmployeeEarning = GetEarning(orderService, percent),
            OrderServiceId = orderService.Id,
        };
    }

    private static (decimal Precent, string Description) GetPrecent(OrderService orderService)
    {
        var precent = orderService.Service.StandartPrecentForEmployee;
        var descriptionBuilder = $"Стандартный процент за выполнение услуги 1 сотрудником";
        
        if (orderService.OrderServiceEmployees.Count > 1)
        {
            if (orderService.Order.DateStart.TimeOfDay.Hours > TimeSpan.FromHours(18).Hours)
            {
                precent = orderService.Service.PrecentLaterOrderForMultipleEmployeers;
                descriptionBuilder = $"Процент за выполнение услуги после 18:00 несколькими сотрудниками";
            }
            else
            {
                precent = orderService.Service.PrecentForMultipleEmployeers;
                descriptionBuilder = $"Процент за выполнение услуги нескольким сотрудником";
            }
        }
        else
        {
            if (orderService.Order.DateStart.TimeOfDay.Hours > TimeSpan.FromHours(18).Hours)
            {
                precent = orderService.Service.PrecentLaterOrderForEmployee;
                descriptionBuilder = $"Процент за выполнение услуги после 18:00 1 сотрудником";
            }
        }

        return (precent, descriptionBuilder);
    }

    private static decimal GetEarning(OrderService orderService, decimal precent)
    {
        var percentForEmployee = Math.Ceiling(precent / orderService.OrderServiceEmployees.Count);

        return orderService.PriceTotal * percentForEmployee/100.0M;
    }
}
