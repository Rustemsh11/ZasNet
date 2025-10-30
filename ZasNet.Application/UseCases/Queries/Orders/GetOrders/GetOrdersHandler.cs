using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrders;

public class GetOrdersHandler(IRepositoryManager repositoryManager, IMapper mapper) : IRequestHandler<GetOrdersRequest, List<GetOrdersResponse>>
{
    public async Task<List<GetOrdersResponse>> Handle(GetOrdersRequest request, CancellationToken cancellationToken)
    {
        var orders = await repositoryManager.OrderRepository.FindAll(false).ToListAsync(cancellationToken);

        return mapper.Map<List<GetOrdersResponse>>(orders);
    }
}
