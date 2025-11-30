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
            .ForMember(c => c.OrderServicesDtos, c => c.MapFrom(src =>
                    src.OrderServices.Select(c => new OrderServiceDto(c.Id, c.ServiceId, c.Price, c.TotalVolume,
                    c.OrderServiceEmployees.Select(x=> new OrderServiceEmployeeDto() 
                    {
                        Employee = new EmployeeDto()
                        {
                            Id =x.EmployeeId,
                            Name = x.Employee.Name,
                        },
                        OrderServiceId = c.Id,
                        IsApproved = x.IsApproved,
                    }).ToList(),
                    c.OrderServiceCars.Select(x => new OrderServiceCarDto()
                    {
                        Car = new CarDto()
                        {
                            Id =x.CarId,
                            Name = $"{x.Car.CarModel.Name}({x.Car.Number})",
                        },
                        OrderServiceId = c.Id,
                        IsApproved = x.IsApproved,
                    }).ToList()))));;

        CreateMap<OrderDto, Order.UpsertOrderDto>()
            .ForMember(c=>c.CreatedEmployeeId, c=>c.MapFrom(src=> src.CreatedUser.Id))
            .ForMember(c=>c.Status, c=>c.MapFrom(src=> src.Status))
            .ForMember(c=>c.OrderServices, c=>c.MapFrom(src=>
                src.OrderServicesDtos.Select(c=> new OrderService() 
                { 
                    Id = c.Id,
                    ServiceId = c.ServiceId, 
                    Price = c.Price, 
                    TotalVolume = c.TotalVolume, 
                    PriceTotal = 
                    c.Price * (decimal)c.TotalVolume, 
                    OrderId = src.Id,
                    OrderServiceCars = c.OrderServiceCarDtos.Select(c=> new OrderServiceCar()
                    {
                        OrderServiceId = c.OrderServiceId,
                        CarId = c.Car.Id,
                        IsApproved = c.IsApproved,
                    }).ToList(),
                    OrderServiceEmployees = c.OrderServiceEmployeeDtos.Select(c=> new OrderServiceEmployee()
                    {
                        OrderServiceId = c.OrderServiceId,
                        EmployeeId = c.Employee.Id,
                        IsApproved= c.IsApproved,
                    }).ToList()
                })));
    }
}
