using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrders;

public class GetOrdersHandler(IRepositoryManager repositoryManager, IMapper mapper) : IRequestHandler<GetOrdersRequest, List<GetOrdersResponse>>
{
    public async Task<List<GetOrdersResponse>> Handle(GetOrdersRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await repositoryManager.OrderRepository.FindAll(false)
                .Include(c=>c.OrderEmployees).ThenInclude(c=>c.Employee)
                .Include(c=>c.OrderServices).ThenInclude(c=>c.Service)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<GetOrdersResponse>>(orders);
        }
        catch (Exception ex)
        {


        }
        return Array.Empty<GetOrdersResponse>().ToList();
    }
}
