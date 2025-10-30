using AutoMapper;
using ZasNet.Application.UseCases.Queries.Orders.GetOrders;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.Mappers;

public class OrderMapper: Profile
{
    public OrderMapper()
    {
        CreateMap<Order, GetOrdersResponse>()
            .ForMember(c => c.Address, (c) => c.MapFrom(src => $"{src.AddressCity}, {src.AddressStreet} {src.AddressNumber}"))
            .ForMember(c => c.Employee, c => c.MapFrom(src => src.OrderEmployees.First().Employee.Name))
            .ForMember(c => c.ServiceNames, c => c.MapFrom(src => string.Join(", ", src.OrderServices.Select(c => c.Service.Name))));
    }
}
