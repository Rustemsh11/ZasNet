using AutoMapper;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Queries.Orders.GetOrders;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.Mappers;

public class OrderMapper: Profile
{
    public OrderMapper()
    {
        CreateMap<Order, GetOrdersResponse>();
        CreateMap<Order, OrderDto>()
            .ForMember(c => c.OrderEmployeeDtos, c => c.MapFrom(src =>
                    src.OrderEmployees.Select(c => new EmployeeDto() { Id = c.EmployeeId, Name = c.Employee.Name })))
            .ForMember(c => c.OrderCarDtos, c => c.MapFrom(src =>
                    src.OrderCars.Select(c => new CarDto() { Id = c.CarId, Name = $"{c.Car.CarModel.Name} ({c.Car.Number})" })))
        .ForMember(c => c.OrderServicesDtos, c => c.MapFrom(src =>
                    src.OrderServices.Select(c => new OrderServiceDto(c.ServiceId, c.Price, c.TotalVolume))));
    }
}
