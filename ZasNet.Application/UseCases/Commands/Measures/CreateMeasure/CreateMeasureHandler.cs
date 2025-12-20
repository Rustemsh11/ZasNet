using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Measures.CreateMeasure;

public class CreateMeasureHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateMeasureRequest>
{
    public async Task Handle(CreateMeasureRequest request, CancellationToken cancellationToken)
    {
        var measureRepo = repositoryManager.MeasureRepository;

        var matchs = await measureRepo.FindByCondition(c => c.Name.ToLower() == request.Name.ToLower(), false).ToListAsync(cancellationToken);
        if(matchs.Count > 0)
        {
            throw new InvalidOperationException("Единица измерения с таким названием уже существует");
        }

        var measure = Measure.Create(request.Name);

        measureRepo.Create(measure);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

