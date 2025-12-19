namespace ZasNet.Application.CommonDtos;

public class EmployeeEarningDto
{
    public int EmployeeEarningId { get; set; }

    public decimal ServiceEmployeePrecent { get; set; }

    public string? PrecentEmployeeDescription { get; set; }

    public decimal EmployeeEarning { get; set; }
}
