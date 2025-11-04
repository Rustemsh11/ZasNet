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
            .ForMember(c => c.Employee, c => c.MapFrom(src => 
                src.OrderEmployees != null && src.OrderEmployees.Any() 
                    ? (src.OrderEmployees.First().Employee != null ? src.OrderEmployees.First().Employee.Name : string.Empty)
                    : string.Empty))
            .ForMember(c => c.ServiceNames, c => c.MapFrom(src => src.OrderServices != null && src.OrderServices.Any()
                ? src.OrderServices
                    .Where(os => os.Service != null)
                    .Select(os => os.Service.Name)
                    .ToList()
                : new List<string>()));
    }
}
