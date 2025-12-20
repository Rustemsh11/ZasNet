using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Measures.GetMeasures;

public class GetMeasuresHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetMeasuresRequest, List<MeasureDto>>
{
    public async Task<List<MeasureDto>> Handle(GetMeasuresRequest request, CancellationToken cancellationToken)
    {
        return await repositoryManager.MeasureRepository.FindAll(false)
            .Select(c => new MeasureDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);
    }
}

