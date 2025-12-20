using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Measures.DeleteMeasure;

public class DeleteMeasureHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteMeasureCommand>
{
    public async Task Handle(DeleteMeasureCommand request, CancellationToken cancellationToken)
    {
        var measure = await repositoryManager.MeasureRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Единицы измерения с id [{request.Id}] не найдено");

        measure.IsDeleted = true;

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

