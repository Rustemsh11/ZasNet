using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Measures.UpdateMeasure;

public class UpdateMeasureHandler(IRepositoryManager repositoryManager) : IRequestHandler<UpdateMeasureCommand>
{
    public async Task Handle(UpdateMeasureCommand request, CancellationToken cancellationToken)
    {
        var measureRepo = repositoryManager.MeasureRepository;

        var measure = await measureRepo.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);
        if (measure == null)
        {
            throw new InvalidOperationException($"Единицы измерения с id: {request.Id} не существует");
        }

        measure.UpdateMeasure(request.Name);

        measureRepo.Update(measure);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

