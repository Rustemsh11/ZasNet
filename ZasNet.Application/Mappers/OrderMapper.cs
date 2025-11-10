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
            .ForMember(c => c.CreatedUser, c => c.MapFrom(src =>
                    new EmployeeDto() { Id = src.CreatedEmployeeId, Name = src.CreatedEmployee.Name}))
            .ForMember(c => c.OrderEmployeeDtos, c => c.MapFrom(src =>
                    src.OrderEmployees.Select(c => new EmployeeDto() { Id = c.EmployeeId, Name = c.Employee.Name })))
            .ForMember(c => c.OrderCarDtos, c => c.MapFrom(src =>
                    src.OrderCars.Select(c => new CarDto() { Id = c.CarId, Name = $"{c.Car.CarModel.Name} ({c.Car.Number})" })))
            .ForMember(c => c.OrderServicesDtos, c => c.MapFrom(src =>
                    src.OrderServices.Select(c => new OrderServiceDto(c.ServiceId, c.Price, c.TotalVolume))));

        CreateMap<OrderDto, Order.UpsertOrderDto>()
            .ForMember(c=>c.CreatedEmployeeId, c=>c.MapFrom(src=> src.CreatedUser.Id))
            .ForMember(c=>c.OrderEmployees, c=>c.MapFrom(src=>
                src.OrderEmployeeDtos.Select(c=> new OrderEmployee() { EmployeeId = c.Id, OrderId = src.Id})))
            .ForMember(c=>c.OrderCars, c=>c.MapFrom(src=>
                src.OrderCarDtos.Select(c=> new OrderCar() { CarId = c.Id, OrderId = src.Id})))
            .ForMember(c=>c.OrderServices, c=>c.MapFrom(src=>
                src.OrderServicesDtos.Select(c=> new OrderService() { ServiceId = c.ServiceId, Price = c.Price, TotalVolume = c.TotalVolume, PriceTotal = c.Price * (decimal)c.TotalVolume, OrderId = src.Id})));
    }
}
