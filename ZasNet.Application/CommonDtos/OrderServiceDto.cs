namespace ZasNet.Application.CommonDtos;

public record OrderServiceDto(
    int Id,
    int ServiceId,
    decimal Price,
    double TotalVolume,
    decimal StandartPrecentForEmployee,
    decimal PrecentForMultipleEmployeers,
    decimal PrecentLaterOrderForEmployee,
    decimal PrecentLaterOrderForMultipleEmployeers,
    List<OrderServiceEmployeeDto> OrderServiceEmployeeDtos,
    List<OrderServiceCarDto> OrderServiceCarDtos);
