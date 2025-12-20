using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Services.GetServices;

public class GetServicesHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetServicesRequest, List<ServiceDto>>
{
    public async Task<List<ServiceDto>> Handle(GetServicesRequest request, CancellationToken cancellationToken)
    {
        return await repositoryManager.ServiceRepository.FindAll(false)
            .Include(c => c.Measure)
            .Select(c => new ServiceDto()
            {
                Id = c.Id,
                Name = c.Name,
                MinPrice = c.Price,
                Measure = c.Measure.Name,
                MinVolume = c.MinVolume,
                StandartPrecentForEmployee = c.StandartPrecentForEmployee,
                PrecentForMultipleEmployeers = c.PrecentForMultipleEmployeers,
                PrecentLaterOrderForEmployee = c.PrecentLaterOrderForEmployee,
                PrecentLaterOrderForMultipleEmployeers = c.PrecentLaterOrderForMultipleEmployeers
            })
            .ToListAsync(cancellationToken);
    }
}

