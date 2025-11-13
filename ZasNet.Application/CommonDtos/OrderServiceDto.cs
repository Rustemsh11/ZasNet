namespace ZasNet.Application.CommonDtos;

public record OrderServiceDto(int Id, int ServiceId, decimal Price, double TotalVolume, List<OrderServiceEmployeeDto> OrderServiceEmployeeDtos, List<OrderServiceCarDto> OrderServiceCarDtos);
