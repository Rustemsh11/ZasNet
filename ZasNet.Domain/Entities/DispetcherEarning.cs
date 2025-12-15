namespace ZasNet.Domain.Entities;

public class DispetcherEarning : BaseItem
{
    public int OrderId { get; set; }

    public decimal ServiceEmployeePrecent { get; set; }

    public string? PrecentEmployeeDescription { get; set; }

    public decimal EmployeeEarning { get; set; }

    public Order Order { get; set; }

    public static DispetcherEarning CreateDispetcherEarning(decimal precent, decimal priceAmount)
    {
        return new DispetcherEarning()
        {
            ServiceEmployeePrecent = precent,
            PrecentEmployeeDescription = "Стандартный процент для диспетчера",
            EmployeeEarning = priceAmount * precent / 100M
        };
    }
    
    public void UpdateDispetcherEarning(decimal precent, decimal priceAmount)
    {
        ServiceEmployeePrecent = precent;
        PrecentEmployeeDescription = "Стандартный процент для диспетчера";
        EmployeeEarning = priceAmount * precent / 100M;
    }


}
