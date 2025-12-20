using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.CarModels.GetCarModels;

public class GetCarModelsHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetCarModelsRequest, List<CarModelDto>>
{
    public async Task<List<CarModelDto>> Handle(GetCarModelsRequest request, CancellationToken cancellationToken)
    {
        return await repositoryManager.CarModelRepository.FindAll(false)
            .Select(c => new CarModelDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);
    }
}

