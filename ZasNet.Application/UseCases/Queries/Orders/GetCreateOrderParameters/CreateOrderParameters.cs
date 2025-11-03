using ZasNet.Application.CommonDtos;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetCreateOrderParameters;

public class CreateOrderParameters
{
    public List<ServiceDto> ServiceDtos { get; set; }
    public List<EmployeeDto> EmployeeDtos { get; set; }
    public List<CarDto> CarDtos { get; set; }
    public List<PaymentType> PaymentTypes { get; set; }
}
