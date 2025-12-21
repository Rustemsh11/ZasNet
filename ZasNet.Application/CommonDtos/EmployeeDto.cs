namespace ZasNet.Application.CommonDtos;

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public decimal? DispetcherProcent { get; set; }
    public RoleDto? Role { get; set; }
}
